using System;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class HemoResultSearch : PatientBaseSearcher<HemoRecordResult>
    {
        private readonly TimeZoneInfo tz;

        public HemoResultSearch(TimeZoneInfo tz) : base((HemoRecordResult h) => h.Patient)
        {
            this.tz = tz;
        }

        protected override Expression<Func<HemoRecordResult, bool>> ParseDefault(string whereString)
        {
            if (DateTime.TryParse(whereString, out DateTime target))
            {
                var dTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(target, TimeSpan.Zero), tz);
                var lowerBound = dTz.ToUtcDate();
                var upperBound = lowerBound.AddDays(1);
                return (HemoRecordResult r) => r.Record.Created.Value >= lowerBound && r.Record.Created.Value < upperBound;
            }

            return base.ParseDefault(whereString);
        }

        protected override Expression<Func<HemoRecordResult, bool>> AdditionalSearchUsingToken(string key, string op, string value)
        {
            switch (key)
            {
                case "date":
                    if (DateTime.TryParse(value, out DateTime targetDatetime))
                    {
                        var dTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(targetDatetime, TimeSpan.Zero), tz);
                        var lowerBound = dTz.ToUtcDate();
                        var upperBound = lowerBound.AddDays(1);
                        if (op == "=")
                        {
                            return (HemoRecordResult r) => r.Record.Created.Value >= lowerBound && r.Record.Created.Value < upperBound;
                        }
                        if (op == "<")
                        {
                            return (HemoRecordResult r) => r.Record.Created.Value < lowerBound;
                        }
                        if (op == ">")
                        {
                            return (HemoRecordResult r) => r.Record.Created.Value >= upperBound;
                        }
                    }
                    break;
                case "mode":
                    if (Enum.TryParse(typeof(DialysisMode), value, true, out object modeValue))
                    {
                        return (HemoRecordResult r) => r.Prescription.Mode == (DialysisMode)modeValue;
                    }
                    break;
                default:
                    break;
            }

            return null;
        }
    }
}
