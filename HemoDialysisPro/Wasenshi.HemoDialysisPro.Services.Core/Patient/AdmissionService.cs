using System;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class AdmissionService : IAdmissionService
    {
        private readonly IAdmissionRepository admissionRepo;

        public AdmissionService(
            IAdmissionRepository admissionRepo)
        {
            this.admissionRepo = admissionRepo;
        }

        public Page<Admission> GetAllAdmissions(int page = 1, int limit = 25, Action<IOrderer<Admission>> orderBy = null, Expression<Func<Admission, bool>> whereCondition = null)
        {
            var query = admissionRepo.GetAll();
            void Ordering(IOrderer<Admission> order)
            {
                order.Default(x => x.Created, true); // Default order by latest admit
                orderBy?.Invoke(order); // Followed by custom ordering from client or controller
            }

            var pageResult = query.GetPagination(limit, page - 1, Ordering, whereCondition);

            return pageResult;
        }

        public Page<Admission> GetAdmissionForPatient(string patientId, int page = 1, int limit = 25)
        {
            var query = admissionRepo.GetAll();

            var pageResult = query.GetPagination(limit, page - 1, (orderer) => orderer.OrderBy(p => p.Admit, true), x => x.PatientId == patientId);

            return pageResult;
        }

        public Admission FindAdmission(Expression<Func<Admission, bool>> expression)
        {
            Admission admit = admissionRepo.Find(expression).FirstOrDefault();

            return admit;
        }

        public Admission GetAdmission(Guid id)
        {
            return admissionRepo.Get(id);
        }

        public Admission CreateNewAdmission(Admission admission)
        {
            CheckAvailability(admission);

            admissionRepo.Insert(admission);

            admissionRepo.Complete();

            return admission;
        }

        public bool UpdateAdmission(Admission admission)
        {
            CheckAvailability(admission);

            admissionRepo.SyncCollection(admission, x => x.Underlying, new AdmissionUnderlying(), onNew: x => x.AdmissionId = admission.Id);
            admissionRepo.Update(admission);

            return admissionRepo.Complete() > 0;
        }

        public bool DeleteAdmission(Guid id)
        {
            admissionRepo.Delete(new Admission { Id = id });

            var result = admissionRepo.Complete();

            return result > 0;
        }

        // ============================ Admission Validation ============================
        void CheckAvailability(Admission admission)
        {
            if (admission.Discharged.HasValue && admission.Discharged < admission.Admit)
            {
                throw new AppException("DC", "Discharged date cannot be earlier than admit date");
            }

            DateTime lowerLimit = admission.Admit;
            DateTime? upperLimit = admission.Discharged;

            bool overlapsed = false;
            if (upperLimit.HasValue)
            {
                overlapsed = admissionRepo.GetAll(false).Any(x => x.Id != admission.Id && x.PatientId == admission.PatientId &&
                                                ((x.Discharged == null && x.Admit < upperLimit) || x.Discharged.Value >= lowerLimit));
            }
            else
            {
                overlapsed = admissionRepo.GetAll(false).Any(x => x.Id != admission.Id && x.PatientId == admission.PatientId &&
                                                (x.Discharged == null || x.Discharged.Value >= lowerLimit));
            }

            if (overlapsed)
            {
                throw new AppException("OVERLAPSE", "The admissions are overlapsed");
            }
        }
    }
}
