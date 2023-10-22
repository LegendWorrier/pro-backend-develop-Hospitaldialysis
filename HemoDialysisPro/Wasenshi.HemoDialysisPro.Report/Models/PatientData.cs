using MessagePack;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class PatientData : Patient
    {
        [MessagePack.IgnoreMember]
        private readonly IUserResolver userResolver;
        [MessagePack.IgnoreMember]
        private readonly IConfiguration config;

        public PatientData(IUserResolver userResolver, IConfiguration config)
        {
            this.userResolver = userResolver;
            this.config = config;
        }

        [SerializationConstructor]
        public PatientData()
        {
            // for serialization
        }

        private TimeZoneInfo tz => TimezoneUtils.GetTimeZone(config["TIMEZONE"]);

        public int Age => (int)((DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz)).DayNumber - BirthDate.DayNumber) / 365.242199);

        public string HN => HospitalNumber;
        public string Sex => Gender == "M" ? "Male" : Gender == "F" ? "Female" : "Not Specified";

        public string Coverage => CoverageScheme.HasValue ? CoverageScheme switch
        {
            CoverageSchemeType.Cash => config["ReportMapping:Coverage:Cash"],
            CoverageSchemeType.Government => config["ReportMapping:Coverage:Gov"],
            CoverageSchemeType.NationalHealthSecurity => config["ReportMapping:Coverage:Nation"],
            CoverageSchemeType.SocialSecurity => config["ReportMapping:Coverage:Social"],
            CoverageSchemeType.Other => config["ReportMapping:Coverage:Other"],
            _ => throw new InvalidOperationException("Invalid coverage type."),
        } : null;

        public string DoctorName
        {
            get
            {
                if (!DoctorId.HasValue)
                {
                    return "No Doctor";
                }
                var doctor = userResolver.Repo.GetAll(false).FirstOrDefault(x => x.Id == DoctorId.Value);
                if (doctor == null)
                {
                    return "Unknown";
                }

                return userResolver.GetName(doctor);
            }
        }

        public string[] Allergies => Allergy.Select(x => x.Medicine.Name).ToArray();
    }
}
