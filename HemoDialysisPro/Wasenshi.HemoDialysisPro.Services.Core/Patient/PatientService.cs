using System;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.MappingModels;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Repository.Interfaces;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository repository;
        private readonly IPatientHistoryRepository historyRepo;
        private readonly IPatientUnitOfWork uow;
        private readonly IAdmissionRepository admissionRepo;
        private readonly IDialysisPrescriptionRepository prescriptionRepo;
        private readonly IScheduleSectionRepository sectionRepo;

        public PatientService(
            IPatientRepository repository,
            IPatientHistoryRepository historyRepo,
            IPatientUnitOfWork uow,
            IAdmissionRepository admissionRepo,
            IDialysisPrescriptionRepository prescriptionRepo,
            IScheduleSectionRepository sectionRepo)
        {
            this.repository = repository;
            this.historyRepo = historyRepo;
            this.uow = uow;
            this.admissionRepo = admissionRepo;
            this.prescriptionRepo = prescriptionRepo;
            this.sectionRepo = sectionRepo;
        }

        public Page<Patient> GetAllPatients(int page = 1, int limit = 25, Action<IOrderer<Patient>> orderBy = null, Expression<Func<Patient, bool>> whereCondition = null)
        {
            var query = repository.GetAll();
            void Ordering(IOrderer<Patient> order)
            {
                order.Default(x => x.Name); // Default order by name
                orderBy?.Invoke(order); // Followed by custom ordering from client or controller
            }

            var pageResult = query.GetPagination(limit, page - 1, Ordering, whereCondition);

            return pageResult;
        }

        public Page<Patient> GetUnitPatients(int unitId, int page = 1, int limit = 25, Expression<Func<Patient, bool>> whereCondition = null)
        {
            var query = repository.GetAll();
            if (whereCondition != null)
            {
                query = query.Where(whereCondition);
            }

            var pageResult = query.GetPagination(limit, page - 1, (orderer) => orderer.OrderBy(p => p.Name), x => x.UnitId == unitId);

            return pageResult;
        }

        public Page<Patient> GetDoctorPatients(Guid doctorId, int page = 1, int limit = 25, Action<IOrderer<Patient>> orderBy = null, Expression<Func<Patient, bool>> whereCondition = null)
        {
            Expression<Func<Patient, bool>> doctorFilterExpression = p => p.DoctorId == doctorId;
            Expression<Func<Patient, bool>> finalCondition = whereCondition == null ?
                doctorFilterExpression :
                doctorFilterExpression.AndAlso(whereCondition);
            return GetAllPatients(page, limit, orderBy, finalCondition);
        }

        public int CountAll(Expression<Func<Patient, bool>> whereCondition = null)
        {
            IQueryable<Patient> getAll = repository.GetAll(false);
            if (whereCondition != null)
            {
                getAll = getAll.Where(whereCondition);
            }
            return getAll.Count();
        }

        public Patient FindPatient(Expression<Func<Patient, bool>> expression)
        {
            Patient patient = repository.Find(expression).FirstOrDefault();

            return patient;
        }

        public Patient GetPatient(string id)
        {
            var patient = repository.Get(id);
            return patient;
        }

        public Patient GetPatientByRFID(string rfid)
        {
            var patient = repository.Find(x => x.RFID == rfid).FirstOrDefault();
            return patient;
        }

        public Patient CreateNewPatient(Patient patient)
        {
            var old = repository.Get(patient.Id);
            if (old != null)
            {
                throw new PatientException($"this id has already existed. ({patient.Id})");
            }

            repository.Insert(patient);

            repository.Complete();

            return patient;
        }

        public bool UpdatePatient(Patient patient, string newId = null)
        {
            Patient old = uow.Patient.GetAll(false).FirstOrDefault(x => x.Id == patient.Id) ?? throw new AppException("NULL", "Patient not found.");
            var oldId = patient.Id;
            bool updateId = !string.IsNullOrWhiteSpace(newId);
            if (updateId) // change Id
            {
                if (uow.Patient.Get(newId) != null)
                {
                    throw new PatientException($"this id has already existed. ({newId})");
                }

                patient.Id = newId;
                uow.Patient.Insert(patient);
                // update all existing admission
                var existingAdmits = admissionRepo.GetAll(false).Where(x => x.PatientId == oldId).ToList();
                foreach (var item in existingAdmits)
                {
                    item.PatientId = newId;
                    item.IsSystemUpdate = true;
                    admissionRepo.Update(item);
                }
                // update all existing hemosheet
                var existingHemoList = uow.HemoRecord.GetAll(false).Where(x => x.PatientId == oldId).ToList();
                foreach (var item in existingHemoList)
                {
                    item.PatientId = newId;
                    item.IsSystemUpdate = true;
                    uow.HemoRecord.Update(item);
                }
                // update all existing dialysis prescription
                var existingPresList = prescriptionRepo.GetAll(false).Where(x => x.PatientId == oldId).ToList();
                foreach (var item in existingPresList)
                {
                    item.PatientId = newId;
                    item.IsSystemUpdate = true;
                    prescriptionRepo.Update(item);
                }
                // update all existing med
                var existingMedPresList = uow.MedPres.GetAll(false).Where(x => x.PatientId == oldId).ToList();
                foreach (var item in existingMedPresList)
                {
                    item.PatientId = newId;
                    item.IsSystemUpdate = true;
                    uow.MedPres.Update(item);
                }
                // update all schedule
                var existingSchedules = uow.Schedule.GetAll(false).Where(x => x.PatientId == oldId).ToList();
                foreach (var item in existingSchedules)
                {
                    item.PatientId = newId;
                    item.IsSystemUpdate = true;
                    uow.Schedule.Update(item);
                }
                var existingSlots = uow.Slot.GetAll(false).Where(x => x.PatientId == oldId).ToList();
                foreach (var item in existingSlots)
                {
                    uow.Slot.Delete(item);
                    if (old.UnitId == patient.UnitId)
                        uow.Slot.Insert(new SectionSlotPatient { PatientId = newId, SectionId = item.SectionId, Slot = item.Slot, IsSystemUpdate = true });
                }

                uow.Patient.Delete(new Patient { Id = oldId });
            }
            else
            {
                uow.Patient.SyncCollection<Tag, Guid>(patient, x => x.Tags);
                uow.Patient.SyncCollection(patient, x => x.Allergy, new PatientMedicineComparer<Allergy>(), onNew: (x) => x.PatientId = patient.Id);

                uow.Patient.Update(patient);
            }

            // check and update schedules in case of unit transfer
            if (old.UnitId != patient.UnitId)
            {
                var currentSlots = uow.Slot.GetAll(false)
                    .Join(sectionRepo.GetAll(false), x => x.SectionId, x => x.Id, (slot, section) => new { Slot = slot, Section = section })
                    .Where(x => x.Slot.PatientId == patient.Id && x.Section.UnitId != patient.UnitId)
                    .Select(x => x.Slot);
                foreach (var item in currentSlots)
                {
                    uow.Slot.Delete(item);
                }
                var conflictedSchedules = uow.Schedule.Find(x => x.PatientId == patient.Id && x.OverrideUnitId == patient.UnitId).ToList();
                foreach (var item in conflictedSchedules)
                {
                    item.OverrideUnitId = null;
                    uow.Schedule.Update(item);
                }
            }

            var result = uow.Complete();

            if (result > 0 && updateId)
            {
                ServiceEvents.DispatchPatientIdUpdate(oldId, newId);
            }

            return result > 0;
        }

        public bool DeletePatient(string id)
        {
            repository.Delete(new Patient { Id = id });

            var result = repository.Complete();

            return result > 0;
        }

        public IEnumerable<PatientHistory> GetPatientHistory(string patientId)
        {
            return historyRepo.GetAll(false).Where(x => x.PatientId == patientId).ToList();
        }

        public bool UpdatePatientHistory(string patientId, IEnumerable<PatientHistory> entries)
        {
            foreach (var item in entries)
            {
                item.PatientId = patientId;
            }
            historyRepo.CreateOrUpdateBatch(entries);
            var result = historyRepo.Complete();

            return result > 0;
        }

        #region Exception
        [Serializable]
        public class PatientException : Exception
        {
            public PatientException() { }
            public PatientException(string message) : base(message) { }
            public PatientException(string message, Exception inner) : base(message, inner) { }
            protected PatientException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        #endregion

    }
}
