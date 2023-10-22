using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class SystemBoundLabProcessor : ISystemBoundLabProcessor
    {
        private readonly ILabUnitOfWork labUOW;
        private readonly IConfiguration config;
        private static int KTV_ID;
        private static int URR_ID;

        private static int BUN_ID;

        public SystemBoundLabProcessor(ILabUnitOfWork labUOW, IConfiguration config)
        {
            this.labUOW = labUOW;
            this.config = config;
            if (KTV_ID == 0)
            {
                KTV_ID = labUOW.LabMaster.Find(x => x.Bound == Models.Enums.SpecialLabItem.KtV).First().Id;
                URR_ID = labUOW.LabMaster.Find(x => x.Bound == Models.Enums.SpecialLabItem.URR).First().Id;
                BUN_ID = labUOW.LabMaster.Find(x => x.Bound == Models.Enums.SpecialLabItem.BUN).First().Id;
            }
        }

        /// <summary>
        /// Update hemosheet case, only calculate / update KTV value.
        /// </summary>
        /// <param name="hemosheet"></param>
        public void ProcessBUN(HemodialysisRecord hemosheet)
        {
            TimeZoneInfo tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var patientId = hemosheet.PatientId;
            var entryDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(hemosheet.Created.Value, TimeSpan.Zero), tz);
            var lowerLimit = entryDate.ToUtcDate();
            var upperLimit = lowerLimit.AddDays(1);

            var BUN = labUOW.LabExam.GetAll(false).Where(x =>
                            x.PatientId == patientId &&
                            x.LabItemId == BUN_ID &&
                            x.EntryTime > lowerLimit &&
                            x.EntryTime < upperLimit)
                .OrderBy(x => x.EntryTime)
                .ToList();

            if (BUN.Count > 1)
            {
                LabExam pre = BUN.First();
                LabExam post = BUN.Last();

                LabExam ktv = null;
                UpdateBunCalculated(patientId, entryDate, hemosheet, pre, post, out ktv, out _, false);
                InsertBunCalculated(patientId, pre, post, hemosheet, ktv == null, false);
            }
        }

        public void ProcessBUN(LabExam item, bool forceUpdateCheck)
        {
            TimeZoneInfo tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var entryDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(item.EntryTime, TimeSpan.Zero), tz);
            var lowerLimit = entryDate.ToUtcDate();
            var upperLimit = lowerLimit.AddHours(24);

            var BUN = labUOW.LabExam.GetAll(false).Where(x =>
                            x.Id != item.Id &&
                            x.PatientId == item.PatientId &&
                            x.LabItemId == BUN_ID &&
                            x.EntryTime > lowerLimit &&
                            x.EntryTime < upperLimit).ToList();

            // safe guard : not allow to have more than 2 records per day per patient
            if (BUN.Count >= 2)
            {
                throw new SystemBoundException("Cannot have more than 2 BUN records per day per patient.");
            }
            if (BUN.Count < 1)
            {
                return;
            }

            LabExam pre = BUN[0].EntryTime < item.EntryTime ? BUN[0] : item;
            LabExam post = BUN[0].EntryTime < item.EntryTime ? item : BUN[0];

            HemodialysisRecord hemosheet = labUOW.HemoRecord.Find(x =>
                x.PatientId == item.PatientId &&
                x.DialysisPrescriptionId != null &&
                x.Created.Value > lowerLimit &&
                x.Created.Value < upperLimit)
                .OrderByDescending(x => x.Created)
                .FirstOrDefault();

            LabExam ktv = null;
            LabExam urr = null;
            if (forceUpdateCheck)
            {
                UpdateBunCalculated(item.PatientId, entryDate, hemosheet, pre, post, out ktv, out urr);
            }

            InsertBunCalculated(item.PatientId, pre, post, hemosheet, ktv == null, urr == null);
        }

        private bool KtvCalculatable(HemodialysisRecord hemosheet)
        {
            return hemosheet?.DialysisPrescription != null && hemosheet.Dehydration.PostTotalWeight > 0;
        }

        private float CalculateUfNet(HemodialysisRecord hemosheet)
        {
            if (hemosheet.DialysisPrescription == null)
            {
                return 0;
            }
            var dryWeight = hemosheet.DialysisPrescription.DryWeight ?? hemosheet.DialysisPrescription.ExcessFluidRemovalAmount;
            if (dryWeight == null)
            {
                return 0;
            }
            return hemosheet.Dehydration.PreTotalWeight - dryWeight.Value;
        }

        private float CalculateKtv(LabExam pre, LabExam post, HemodialysisRecord hemosheet)
        {
            var R = post.LabValue / pre.LabValue;
            var t = hemosheet.DialysisPrescription.Duration.TotalHours;
            var uf = CalculateUfNet(hemosheet);
            var w = hemosheet.Dehydration.PostTotalWeight;

            double result = -Math.Log(R - 0.008 * t) + (4 - 3.5 * R) * uf / w;
            return (float)result;
        }

        private float CalculateUrr(LabExam pre, LabExam post)
        {
            return (pre.LabValue - post.LabValue) / pre.LabValue * 100;
        }

        public void CleanBUNCalculation(LabExam item)
        {
            TimeZoneInfo tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var entryDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(item.EntryTime, TimeSpan.Zero), tz);
            DeleteBunCalculated(item.PatientId, entryDate);
        }

        /// <summary>
        /// Update hemosheet case, only delete KTV value.
        /// </summary>
        /// <param name="hemosheet"></param>
        public void CleanBUNCalculation(HemodialysisRecord hemosheet)
        {
            TimeZoneInfo tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var entryDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(hemosheet.Created.Value, TimeSpan.Zero), tz);
            DeleteBunCalculated(hemosheet.PatientId, entryDate, false);
        }

        private void UpdateBunCalculated(string patientId, DateTimeOffset entryTime, HemodialysisRecord hemosheet, LabExam pre, LabExam post, out LabExam ktv, out LabExam urr, bool calUrr = true)
        {
            var lowerLimit = entryTime.ToUtcDate();
            var upperLimit = lowerLimit.AddDays(1);
            // Kt/V
            ktv = labUOW.LabExam.Find(x =>
                        x.PatientId == patientId &&
                        x.LabItemId == KTV_ID &&
                        x.EntryTime > lowerLimit &&
                        x.EntryTime < upperLimit).FirstOrDefault();
            if (ktv != null && KtvCalculatable(hemosheet))
            {
                ktv.LabValue = CalculateKtv(pre, post, hemosheet);
                ktv.IsSystemUpdate = true;
                labUOW.LabExam.Update(ktv);
            }

            if (calUrr)
            {
                // URR
                urr = labUOW.LabExam.Find(x =>
                            x.PatientId == patientId &&
                            x.LabItemId == URR_ID &&
                            x.EntryTime > lowerLimit &&
                            x.EntryTime < upperLimit).FirstOrDefault();
                if (urr != null)
                {
                    urr.LabValue = CalculateUrr(pre, post);
                    urr.IsSystemUpdate = true;
                    labUOW.LabExam.Update(urr);
                }
            }
            else
            {
                urr = null;
            }
        }

        private void InsertBunCalculated(string patientId, LabExam pre, LabExam post, HemodialysisRecord hemosheet, bool calKtv, bool calUrr)
        {
            // Kt/V
            if (calKtv && KtvCalculatable(hemosheet))
            {
                var ktv = new LabExam
                {
                    EntryTime = post.EntryTime,
                    IsSystemUpdate = true,
                    LabItemId = KTV_ID,
                    PatientId = patientId,
                    LabValue = CalculateKtv(pre, post, hemosheet),
                    Note = "This is calculated by the system."
                };
                labUOW.LabExam.Insert(ktv);
            }
            // URR
            if (calUrr)
            {
                var urr = new LabExam
                {
                    EntryTime = post.EntryTime,
                    IsSystemUpdate = true,
                    LabItemId = URR_ID,
                    PatientId = patientId,
                    LabValue = CalculateUrr(pre, post),
                    Note = "This is calculated by the system."
                };
                labUOW.LabExam.Insert(urr);
            }
        }

        private void DeleteBunCalculated(string patientId, DateTimeOffset entryTime, bool includeUrr = true)
        {
            var lowerLimit = entryTime.ToUtcDate();
            var upperLimit = lowerLimit.AddDays(1);

            // Kt/V
            LabExam ktv = labUOW.LabExam.Find(x =>
                        x.PatientId == patientId &&
                        x.LabItemId == KTV_ID &&
                        x.EntryTime > lowerLimit &&
                        x.EntryTime < upperLimit).FirstOrDefault();
            if (ktv != null)
            {
                labUOW.LabExam.Delete(ktv);
            }

            if (includeUrr)
            {
                // URR
                LabExam urr = labUOW.LabExam.Find(x =>
                            x.PatientId == patientId &&
                            x.LabItemId == URR_ID &&
                            x.EntryTime > lowerLimit &&
                            x.EntryTime < upperLimit).FirstOrDefault();
                if (urr != null)
                {
                    labUOW.LabExam.Delete(urr);
                }
            }
        }

        public void Commit()
        {
            labUOW.Complete();
        }
    }
}
