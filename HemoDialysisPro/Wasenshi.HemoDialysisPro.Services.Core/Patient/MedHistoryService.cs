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
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class MedHistoryService : IMedHistoryService
    {
        private readonly IConfiguration config;
        private readonly IMedHistoryRepository medRepo;
        private readonly IMedHistoryProcessor processor;

        public MedHistoryService(
            IConfiguration config,
            IMedHistoryRepository medRepo,
            IMedHistoryProcessor processor)
        {
            this.config = config;
            this.medRepo = medRepo;
            this.processor = processor;
        }

        public Page<MedHistoryItem> GetAllMedHistory(int page = 1, int limit = 25, Action<IOrderer<MedHistoryItem>> orderBy = null, Expression<Func<MedHistoryItem, bool>> condition = null)
        {
            IQueryable<MedHistoryItem> allItems = medRepo.GetAll();

            void Ordering(IOrderer<MedHistoryItem> orderer)
            {
                orderer.Default(x => x.EntryTime, true); // Default order by date with latest first
                orderBy?.Invoke(orderer); // Followed by custom ordering
            }

            var result = allItems.GetPagination(limit, page - 1, Ordering, condition);

            return result;
        }

        public MedHistoryItem GetMedHistory(Guid id)
        {
            return medRepo.Get(id);
        }

        public IEnumerable<MedHistoryItem> CreateMedHistoryBatch(string patientId, DateTime entryTime, List<MedHistoryItem> medItems)
        {
            foreach (var item in medItems)
            {
                item.PatientId = patientId;
                if (item.EntryTime == default)
                {
                    item.EntryTime = entryTime;
                }
            }

            medRepo.CreateBatch(medItems);
            medRepo.Complete();

            return medItems;
        }

        public MedHistoryItem UpdateMedHistory(MedHistoryItem medItem)
        {
            var old = medRepo.Get(medItem.Id);

            // Safe guard, cannot edit labItemId
            if (old.MedicineId != medItem.MedicineId)
            {
                throw new InvalidOperationException("Cannot edit Med History id. Create new one instead.");
            }

            medItem.CreatedBy = old.CreatedBy;
            medItem.Created = old.Created;
            medItem.Medicine = null;

            medRepo.Update(medItem);

            if (medRepo.Complete() > 0)
            {
                return medItem;
            }
            return null;
        }

        public bool DeleteMedHistory(Guid id)
        {
            MedHistoryItem med = medRepo.Get(id);
            if (med == null)
            {
                return false;
            }

            medRepo.Delete(med);

            return medRepo.Complete() > 0;
        }

        public MedHistoryResult GetMedHistoryByPatientId(string patientId, Expression<Func<MedHistoryItem, bool>> prerequisite = null, DateTime? filter = null, DateTime? upperLimit = null)
        {
            Expression<Func<MedHistoryItem, bool>> whereCondition = null;
            DateTime limitDate;
            if (!filter.HasValue)
            {
                // Default limit within 3 months
                limitDate = DateTime.UtcNow.AddMonths(-2);
                limitDate = new DateTime(limitDate.Year, limitDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
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

            var sql = medRepo.GetAll().Where(whereCondition);
            if (upperLimit.HasValue)
            {
                sql = sql.Where(x => x.EntryTime < upperLimit.Value);
            }
            var dataResult = sql.ToList();

            return processor.ProcessData(dataResult);
        }

        public MedOverview GetMedOverviewByPatientId(string patientId)
        {
            TimeZoneInfo tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var current = DateTime.UtcNow;
            var tzTime = TimeZoneInfo.ConvertTime(new DateTimeOffset(current, TimeSpan.Zero), tz)
                        .FirstDayOfMonth();
            var entryLimit = tzTime.ToUtcDate();

            IEnumerable<MedHistoryItem> medList = (from meds in medRepo.GetAll()
                                                   where meds.EntryTime > entryLimit && meds.PatientId == patientId
                                                   select meds).ToList();

            var group = medList.GroupBy(x => x.MedicineId)
                .Select(group =>
                        new
                        {
                            MedId = group.Key,
                            group.First().Medicine.Name,
                            Data = group.OrderByDescending(x => x.EntryTime)
                        })
                  .OrderBy(group => group.Name)
                  .ToList();

            MedOverview result = new()
            {
                PatientId = patientId,
                ThisMonthMeds = group.Select(x => new MedItem
                {
                    MedId = x.MedId,
                    Medicine = x.Data.First().Medicine,
                    Count = x.Data.Count()
                })
            };

            return result;
        }
    }
}
