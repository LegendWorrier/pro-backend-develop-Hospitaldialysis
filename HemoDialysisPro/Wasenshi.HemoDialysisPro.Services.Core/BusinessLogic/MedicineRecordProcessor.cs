using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class MedicineRecordProcessor : IMedicineRecordProcessor
    {
        private readonly IExecutionRecordRepository executionRecordRepo;

        public MedicineRecordProcessor(IExecutionRecordRepository executionRecordRepo)
        {
            this.executionRecordRepo = executionRecordRepo;
        }

        public bool CheckAvailablity(MedicinePrescription prescription, out string reason, TimeZoneInfo timeZone = null)
        {
            if (prescription == null)
            {
                throw new Exception("Prescription is null in checking process.");
            }
            reason = null;
            if (prescription.Frequency == Frequency.PRN)
            {
                return true;
            }

            if (prescription.Frequency == Frequency.ST
                && executionRecordRepo.GetMedicineRecords(false).Any(x => x.PrescriptionId == prescription.Id))
            {
                reason = $"The med is at the expected amount. ({GetFrequencyLabel(prescription.Frequency)})";
                return false;
            }

            if (prescription.Duration > 0)
            {
                var expiredDate = prescription.AdministerDate.AddDays(prescription.Duration);
                if (DateTime.UtcNow >= expiredDate)
                {
                    reason = $"The med prescription has expired. ({expiredDate})";
                    return false;
                }
            }

            int freq = (int)prescription.Frequency;
            var allMeds = executionRecordRepo.GetMedicineRecords(false)
                .Where(x => x.PrescriptionId == prescription.Id)
                .Select(x => new { Time = x.IsExecuted ? x.Timestamp : x.Created }).ToList();
            var offsetTicks = timeZone?.BaseUtcOffset.Ticks ?? 0;

            var today = DateTime.UtcNow.Date.AddTicks(offsetTicks);
            DateTime lastCheckpoint = today;
            int threshold = 0;
            if (freq >= (int)Frequency.QD) // Everyday
            {
                threshold = Math.Max(freq - 1, 0);
            }
            else if (freq >= (int)Frequency.QOD) // Every other day
            {
                var yesterday = DateTime.UtcNow.AddDays(-1);
                yesterday = yesterday.Subtract(yesterday.TimeOfDay).AddTicks(offsetTicks);

                lastCheckpoint = yesterday;
                threshold = freq + 2;
            }
            else if (freq >= (int)Frequency.QW) // N times a week
            {
                var lastWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                lastWeek = lastWeek.Subtract(lastWeek.TimeOfDay).AddTicks(offsetTicks);

                lastCheckpoint = lastWeek;
                threshold = freq + 7;
            }
            else // Every N weeks
            {
                var lastNWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek + (freq + 7));
                lastNWeek = lastNWeek.Subtract(lastNWeek.TimeOfDay).AddTicks(offsetTicks);

                lastCheckpoint = lastNWeek;
                threshold = 0;
            }

            var sinceLastCheckpointMeds = allMeds.Where(x => x.Time >= lastCheckpoint).ToList();
            if (sinceLastCheckpointMeds.Count > threshold)
            {
                reason = $"The med is at the expected amount. ({GetFrequencyLabel(prescription.Frequency)})";
                return false;
            }

            if (freq < (int)Frequency.QD && sinceLastCheckpointMeds.Count(x => x.Time >= today) > 0)
            {
                reason = "The med is already given today.";
                return false;
            }

            return true;
        }

        public static string GetFrequencyLabel(Frequency freq)
        {
            switch (freq)
            {
                case Frequency.QD:
                    return "Once a day";
                case Frequency.QN:
                    return "Once every night";
                case Frequency.BID:
                    return "Two times a day";
                case Frequency.TID:
                    return "Three times a day";
                case Frequency.QID:
                    return "Four times a day";
                case Frequency.QOD:
                    return "Once every other day";
                case Frequency.QW:
                    return "Once a week";
                case Frequency.BIW:
                    return "Two times a week";
                case Frequency.TIW:
                    return "Three times a week";
                case Frequency.QIW:
                    return "Four times a week";
                case Frequency.Q2W:
                    return "Once every other week";
                case Frequency.Q3W:
                    return "Once every 3 weeks";
                case Frequency.Q4W:
                    return "Once every 4 weeks";
                case Frequency.ST:
                    return "Only Once";
                default:
                    throw new Exception("Not supported frequency");
            }
        }
    }
}
