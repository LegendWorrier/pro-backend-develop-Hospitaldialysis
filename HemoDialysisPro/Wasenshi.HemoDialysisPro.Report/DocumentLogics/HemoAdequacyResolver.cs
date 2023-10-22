using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Telerik.Reporting;
using Wasenshi.HemoDialysisPro.Report.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Utils;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Report.DocumentLogics
{
    public class HemoAdequacyResolver : DocResolverBase
    {
        private readonly IMapper mapper;
        private readonly IHemoUnitOfWork hemoUnit;
        private readonly ILabExamRepository labRepo;
        private readonly ISectionSlotPatientRepository slotRepo;
        private readonly IContextAdapter context;

        public HemoAdequacyResolver(
            ILogger<HemoAdequacyResolver> logger,
            IUserResolver userResolver,
            IFileRepository fileRepo,
            IConfiguration config,
            IMapper mapper,
            IHemoUnitOfWork hemoUnit,
            ILabExamRepository labRepo,
            ISectionSlotPatientRepository slotRepo,
            IContextAdapter context
            ) : base(logger, userResolver, fileRepo, config)
        {
            this.mapper = mapper;
            this.hemoUnit = hemoUnit;
            this.labRepo = labRepo;
            this.slotRepo = slotRepo;
            this.context = context;
        }

        public override object GetData(object data)
        {
            var result = new HemoRecordData();
            result.Patient = new PatientData(userResolver, config);
            mapper.Map(data as HemoRecordData, result);
            foreach (var item in result.Records)
            {
                if (item.Id == Guid.Empty)
                {
                    continue;
                }
                item.Dehydration = mapper.Map(item.Dehydration, new DehydrationData(item));
            }
            return result;
        }

        public override async Task<object> PrepareData(IDictionary<string, object> parameters)
        {
            var patientId = (string)parameters["patientId"];
            var month = DateOnly.Parse((string)parameters["month"], CultureInfo.InvariantCulture);
            return await PreparePatientHemoRecordData(patientId, month);
        }

        public override async Task<object> UpdateData(object prevData)
        {
            var data = ((HemoRecordData)prevData);
            var patientId = data.Patient.Id;
            var month = DateOnly.FromDateTime(data.Date);
            return await PreparePatientHemoRecordData(patientId, month);
        }

        public override void ExtraSetup(InstanceReportSource report)
        {
            // Extra setup
        }

        // ===============================================

        private Task<HemoRecordData> PreparePatientHemoRecordData(string patientId, DateOnly month)
        {
            try
            {
                var patient = hemoUnit.Patient.Get(patientId);

                // med history and allergy
                patient.Allergy = hemoUnit.Patient.Allergies.Where(x => x.PatientId == patient.Id)
                    .Select(x => new HemoDialysisPro.Models.MappingModels.Allergy { Medicine = x.Medicine }).ToList();

                var targetMonth = new DateTimeOffset(month.ToDateTime(new TimeOnly()), tz.BaseUtcOffset);
                var lowerLimit = targetMonth.ToUtcDate();
                var upperLimit = lowerLimit.AddMonths(1);
                var list = hemoUnit.HemoRecord.GetAllWithNote()
                    .Where(x => x.PatientId == patientId
                            && x.CompletedTime != null
                            && x.CompletedTime.Value >= lowerLimit
                            && x.CompletedTime.Value < upperLimit
                            )
                    .OrderBy(x => x.CompletedTime)
                    .ToList();
                var result = new HemoRecordData
                {
                    Patient = mapper.Map(patient, new PatientData(userResolver, config)),

                    Records = mapper.Map<List<HemosheetInfo>>(list),

                    Date = month.ToDateTime(new TimeOnly())
                };
                var ktv = labRepo.GetAll().Where(x => x.PatientId == patientId && x.LabItem.IsSystemBound && x.LabItem.Bound == SpecialLabItem.KtV && x.EntryTime > lowerLimit).ToList();
                var urr = labRepo.GetAll().Where(x => x.PatientId == patientId && x.LabItem.IsSystemBound && x.LabItem.Bound == SpecialLabItem.URR && x.EntryTime > lowerLimit).ToList();
                result.Records = result.Records
                    .GroupJoin(ktv, h => h.CompletedTime.Value.Date, l => l.EntryTime.Date, (h, l) =>
                    {
                        h.KtV = l.FirstOrDefault()?.LabValue;
                        return h;
                    })
                    .GroupJoin(urr, h => h.CompletedTime.Value.Date, l => l.EntryTime.Date, (h, l) =>
                    {
                        h.URR = l.FirstOrDefault()?.LabValue;
                        return h;
                    })
                    .ToList();

                var slots = slotRepo.GetAll().Where(x => x.PatientId == patientId).ToList();
                result.HdPerWeek = slots.Count > 0 ? slots.Count : result.Records.FirstOrDefault()?.DialysisPrescription.Frequency ?? 0;

                StringBuilder str = new StringBuilder();
                foreach (var slot in slots.Select(x => x.Slot))
                {
                    double diff = (double)(((int)slot == 6 ? 0 : slot + 1) - (int)DateTime.UtcNow.DayOfWeek);
                    var targetDay = DateTime.UtcNow.AddDays(diff);
                    str.Append(targetDay.ToString("ddd"));
                    str.Append(", ");
                }
                if (str.Length > 0)
                {
                    str.Remove(str.Length - 2, 2);
                }
                result.HdExtraStr = str.ToString();

                // ================= Fixed Lines ======================
                if (!int.TryParse(config["Reports:HemoRecord:FixedLines"], out int fixedLines))
                {
                    fixedLines = 18;
                }
                if (result.Records.Count < fixedLines)
                {
                    int fillingEmptyLine = fixedLines - result.Records.Count;

                    for (int i = 0; i < fillingEmptyLine; i++)
                    {
                        result.Records.Add(new HemosheetInfo());
                    }
                }

                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                logger.LogError($"Report preparing failed: {e.Message} || {e}");
                throw;
            }
        }
    }
}
