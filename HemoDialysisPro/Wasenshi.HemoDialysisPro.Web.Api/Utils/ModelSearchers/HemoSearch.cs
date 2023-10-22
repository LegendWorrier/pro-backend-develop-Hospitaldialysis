using System;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class HemoSearch : ModelSearcher<HemodialysisRecord>
    {
        private readonly TimeZoneInfo tz;

        public HemoSearch(TimeZoneInfo tz)
        {
            this.tz = tz;
        }

        protected override Expression<Func<HemodialysisRecord, bool>> ParseDefault(string whereString)
        {
            if (DateTime.TryParse(whereString, out DateTime target))
            {
                var dTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(target, TimeSpan.Zero), tz);
                var lowerBound = dTz.ToUtcDate();
                var upperBound = lowerBound.AddDays(1);
                return (HemodialysisRecord r) => r.Created.Value >= lowerBound && r.Created.Value < upperBound;
            }
            if (Enum.TryParse(typeof(DialysisMode), whereString, true, out object modeValue))
            {
                return (HemodialysisRecord r) => r.DialysisPrescription.Mode == (DialysisMode)modeValue;
            }

            return null;
        }

        protected override Expression<Func<HemodialysisRecord, bool>> ParseConditionBlock(string whereString)
        {
            var tokens = Tokenize(whereString, false, "=", "<", ">").ToList();
            if (tokens.Count != 3) return null; // invalid syntax
            var key = tokens[0];
            var op = tokens[1];
            var value = tokens[2];

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
                            return (HemodialysisRecord r) => r.CompletedTime != null && r.CompletedTime.Value >= lowerBound && r.CompletedTime.Value < upperBound;
                        }
                        if (op == "<")
                        {
                            return (HemodialysisRecord r) => r.CompletedTime != null && r.CompletedTime.Value < lowerBound;
                        }
                        if (op == ">")
                        {
                            return (HemodialysisRecord r) => r.CompletedTime != null && r.CompletedTime.Value >= upperBound;
                        }
                    }
                    break;
                case "mode":
                    if (Enum.TryParse(typeof(DialysisMode), value, true, out object modeValue))
                    {
                        return (HemodialysisRecord r) => r.DialysisPrescription.Mode == (DialysisMode)modeValue;
                    }
                    break;
                case "patientId":
                    return (HemodialysisRecord r) => r.PatientId == value;
                default:
                    break;
            }

            return null;
        }
    }
}
