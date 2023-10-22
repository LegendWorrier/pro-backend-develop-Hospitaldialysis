using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class MedicinePrescriptionService : IMedicinePrescriptionService
    {
        private readonly IMedicinePrescriptionRepository medPrescriptionRepo;
        private readonly IMedicineRecordProcessor medProcessor;
        private readonly IPatientService patientService;
        private readonly IMasterDataUOW master;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;

        public MedicinePrescriptionService(
            IMedicinePrescriptionRepository medPrescriptionRepo,
            IMedicineRecordProcessor medProcessor,
            IPatientService patientService,
            IMasterDataUOW master,
            IRedisClient redis,
            IMessageQueueClient message)
        {
            this.medPrescriptionRepo = medPrescriptionRepo;
            this.medProcessor = medProcessor;
            this.patientService = patientService;
            this.master = master;
            this.redis = redis;
            this.message = message;
        }

        public IEnumerable<MedicinePrescription> GetMedicinePrescriptionByPatientId(string patientId)
        {
            var result = medPrescriptionRepo.GetAll()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.Created)
                .ToList();

            return result;
        }

        public IEnumerable<Guid> GetMedicinePrescriptionAutoList(string patientId, TimeZoneInfo timeZone = null)
        {
            var prescriptionList = medPrescriptionRepo.GetAll(false)
                .Where(x => x.PatientId == patientId && x.IsActive)
                .OrderByDescending(n => n.Created)
                .ToList();

            List<Guid> result = new();
            foreach (var g in prescriptionList.GroupBy(x => x.MedicineId))
            {
                var pres = g.FirstOrDefault();
                if (pres != null
                    && pres.Frequency != Frequency.PRN // Never suggest use when needed
                    && medProcessor.CheckAvailablity(pres, out string reason, timeZone)
                    )
                {
                    result.Add(pres.Id);
                }
            }

            return result;
        }

        public MedicinePrescription GetMedicinePrescription(Guid id)
        {
            var result = medPrescriptionRepo.Get(id);
            return result;
        }

        public MedicinePrescription CreateMedicinePrescription(MedicinePrescription record)
        {
            if (!master.GetMasterRepo<Medicine, int>().GetAll(false).Any(x => x.Id == record.MedicineId))
            {
                throw new InvalidOperationException("Medicine Not Found.");
            }

            medPrescriptionRepo.Insert(record);

            medPrescriptionRepo.Complete();

            var med = master.GetMasterRepo<Medicine, int>().Find(x => x.Id == record.MedicineId).First();
            record.Medicine = med;

            PublishNotification(record, NotiType.New);

            return record;
        }

        public bool UpdateMedicinePrescription(MedicinePrescription prescription)
        {
            var old = medPrescriptionRepo.Get(prescription.Id);
            // Safe guard, cannot edit patientId!
            if (old.PatientId != prescription.PatientId)
            {
                throw new InvalidOperationException("Cannot edit patient Id.");
            }
            // Safe guard, cannot edit history!
            if (old.MedicineRecords.Any(x => x.Hemodialysis.CompletedTime != null))
            {
                throw new InvalidOperationException("Cannot edit history.");
            }

            medPrescriptionRepo.Update(prescription);

            var result = medPrescriptionRepo.Complete() > 0;

            if (result)
            {
                PublishNotification(prescription, NotiType.Edit);
            }

            return result;
        }

        public bool DeleteMedicinePrescription(Guid id)
        {
            var prescription = medPrescriptionRepo.Get(id) ?? throw new KeyNotFoundException();

            prescription.IsActive = false;
            medPrescriptionRepo.Update(prescription);

            var result = medPrescriptionRepo.Complete() > 0;

            if (result)
            {
                PublishNotification(prescription, NotiType.Del);
            }

            return result;
        }

        private void PublishNotification(MedicinePrescription prescription, NotiType type)
        {
            if (!FeatureFlag.HasIntegrated())
            {
                return;
            }

            string title, detail, tag;
            switch (type)
            {
                case NotiType.New:
                    title = "MedNew_title";
                    detail = "MedNew_detail::{0}::{1}";
                    tag = "med-new";
                    break;
                case NotiType.Edit:
                    title = "MedEdit_title";
                    detail = "MedEdit_detail::{0}::{1}";
                    tag = "med-edit";
                    break;
                case NotiType.Del:
                    title = "MedDel_title";
                    detail = "MedDel_detail::{0}::{1}";
                    tag = "med-del";
                    break;
                default:
                    throw new InvalidProgramException("Undefined notification type for med pres.");
            }
            var patient = patientService.GetPatient(prescription.PatientId);
            var target = NotificationTarget.ForNurses(patient.UnitId);
            var noti = redis.AddNotification(
                title,
                string.Format(detail, patient.Name, prescription.Medicine.Name),
                new[] { "page", "patient", patient.Id, "med" },
                target,
                tag
                );
            message.SendNotificationEvent(noti, target);
        }

        enum NotiType
        {
            New,
            Edit,
            Del
        }
    }
}
