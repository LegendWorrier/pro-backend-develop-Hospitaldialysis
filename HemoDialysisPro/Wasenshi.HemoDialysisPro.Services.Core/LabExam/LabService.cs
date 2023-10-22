using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class LabService : ILabService
    {
        private readonly IConfiguration config;
        private readonly ILabUnitOfWork labUOW;
        private readonly IPatientRepository patientRepo;
        private readonly IContextAdapter dbContext;
        private readonly ILabExamProcessor processor;
        private readonly ISystemBoundLabProcessor systemBoundLab;

        private static Dictionary<int, SpecialLabItem?> systemBoundMapping;

        public LabService(
            IConfiguration config,
            ILabUnitOfWork labUOW,
            IPatientRepository patientRepo,
            ILabExamProcessor processor,
            IContextAdapter dbContext)
        {
            this.config = config;
            this.labUOW = labUOW;
            this.patientRepo = patientRepo;
            this.processor = processor;
            this.dbContext = dbContext;

            if (systemBoundMapping == null) // init system wide caching only once, on first start
            {
                systemBoundMapping = labUOW.LabMaster.GetAll().Where(x => x.IsSystemBound).ToDictionary(x => x.Id, x => x.Bound);
            }

            systemBoundLab = new SystemBoundLabProcessor(this.labUOW, config);
        }

        public Page<LabExam> GetAllLabExams(int page = 1, int limit = 25,
            Action<IOrderer<LabExam>> orderBy = null,
            Expression<Func<LabExam, bool>> condition = null)
        {
            IQueryable<LabExam> allItems = labUOW.LabExam.GetAll();
            if (condition != null)
            {
                allItems = allItems.Where(condition);
            }

            void Ordering(IOrderer<LabExam> orderer)
            {
                orderer.Default(x => x.EntryTime, true); // Default order by date with latest first
                orderBy?.Invoke(orderer); // Followed by custom ordering
            }

            var result = allItems.GetPagination(limit, page - 1, Ordering);

            return result;
        }

        public LabExam GetLabExam(Guid id)
        {
            return labUOW.LabExam.Get(id);
        }

        public IEnumerable<LabExam> CreateLabExamBatch(string patientId, DateTime entryTime, List<LabExam> labExams)
        {
            List<string> errorList = new List<string>();
            foreach (var item in labExams)
            {
                item.PatientId = patientId;
                item.EntryTime = entryTime;

                try
                {
                    SystemBoundCheck(item);
                }
                catch (SystemBoundException e)
                {
                    errorList.Add(e.Message);
                }
            }

            if (errorList.Count > 0)
            {
                throw new SystemBoundException(errorList);
            }

            labUOW.LabExam.CreateBatch(labExams);
            labUOW.LabExam.Complete();

            return labExams;
        }

        public LabExam UpdateLabExam(LabExam labExam)
        {
            var old = labUOW.LabExam.Get(labExam.Id);

            // Safe guard, cannot edit labItemId
            if (old.LabItemId != labExam.LabItemId)
            {
                throw new InvalidOperationException("Cannot edit Lab Item id. Create new one instead.");
            }

            if (old.EntryTime.Date != labExam.EntryTime.Date)
            {
                CleanSystemBound(old);
                SystemBoundCheck(labExam);
            }
            else
            {
                SystemBoundCheck(labExam, true);
            }

            labExam.CreatedBy = old.CreatedBy;
            labExam.Created = old.Created;
            labExam.LabItem = null;

            labUOW.LabExam.Update(labExam);

            if (labUOW.LabExam.Complete() > 0)
            {
                return labExam;
            }
            return null;
        }

        public bool DeleteLabExam(Guid id)
        {
            LabExam lab = labUOW.LabExam.Get(id);
            if (lab == null)
            {
                return false;
            }

            labUOW.LabExam.Delete(lab);
            CleanSystemBound(lab);

            return labUOW.LabExam.Complete() > 0;
        }

        public LabExamResult GetLabExamByPatientId(string patientId, Expression<Func<LabExam, bool>> prerequisite = null, DateTime? filter = null, DateTime? upperLimit = null)
        {
            Expression<Func<LabExam, bool>> whereCondition = null;
            DateTime limitDate;
            if (!filter.HasValue)
            {
                // Default limit within 3 months
                limitDate = DateTime.UtcNow.AddMonths(-2);
                limitDate = new DateTime(limitDate.Year, limitDate.Month, 1).AsUtcDate();
            }
            else
            {
                limitDate = filter.Value;
            }
            if (upperLimit.HasValue && (filter - upperLimit) > TimeSpan.Zero)
            {
                throw new InvalidOperationException("upper limit cannot be less than filter.");
            }

            whereCondition = x => x.PatientId == patientId && x.EntryTime > limitDate;
            whereCondition = whereCondition.AndAlso(prerequisite);

            var sql = labUOW.LabExam.GetAll().Where(whereCondition);
            if (upperLimit.HasValue)
            {
                sql = sql.Where(x => x.EntryTime < upperLimit.Value);
            }
            var dataResult = sql.ToList();

            return processor.ProcessData(dataResult);
        }

        public Page<LabOverview> GetLabOverview(int page = 1, int limit = 25, Action<IOrderer<LabOverview>> orderBy = null, Expression<Func<LabOverview, bool>> whereCondition = null)
        {
            IQueryable<LabOverview> query = dbContext.Context.LabOverviews.Include(x => x.Patient);

            void Ordering(IOrderer<LabOverview> order)
            {
                order.Default(x => x.Patient.Name); // Default order by name
                orderBy?.Invoke(order); // Followed by custom ordering from client or controller
            }

            var pageResult = query.GetPagination(limit, page - 1, Ordering, whereCondition);
            return pageResult;
        }

        // ======================= Add more business logic here =============================
        // TODO: business logic for built-in lab (system bound lab item)
        private void SystemBoundCheck(LabExam item, bool forceUpdateCheck = false)
        {
            if (systemBoundMapping.ContainsKey(item.LabItemId))
            {
                switch (systemBoundMapping[item.LabItemId])
                {
                    case SpecialLabItem.BUN:
                        systemBoundLab.ProcessBUN(item, forceUpdateCheck);

                        break;
                    default:
                        break;
                }
            }
        }

        private void CleanSystemBound(LabExam item)
        {
            if (systemBoundMapping.ContainsKey(item.LabItemId))
            {
                switch (systemBoundMapping[item.LabItemId])
                {
                    case SpecialLabItem.BUN:
                        systemBoundLab.CleanBUNCalculation(item);

                        break;
                    default:
                        break;
                }
            }
        }

        public IEnumerable<LabHemosheet> GetLabHemosheetList()
        {
            return dbContext.Context.LabHemosheets.Include(x => x.Item).ToList();
        }

        public void AddOrUpdateLabHemosheet(IEnumerable<LabHemosheet> labHemosheets)
        {
            dbContext.Context.LabHemosheets.RemoveRange(dbContext.Context.LabHemosheets);
            dbContext.Context.SaveChanges();

            dbContext.Context.LabHemosheets.AddRange(labHemosheets);

            int result = dbContext.Context.SaveChanges();
            if (result != labHemosheets.Count())
            {
                throw new Exception("Some thing wrong.");
            }
        }
    }
}
