using System;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class PatientBaseSearcher<T> : ModelSearcher<T>
    {
        private readonly ModelReplacer<Patient> replacer;

        public PatientBaseSearcher(Expression<Func<T, Patient>> getPatient)
        {
            replacer = new ModelReplacer<Patient>(getPatient);
        }

        protected override Expression<Func<T, bool>> ParseConditionBlock(string whereString)
        {
            var tokens = Tokenize(whereString, false, new[] { "=", "<", ">" }.Concat(AdditionalOperator()).ToArray()).ToList();
            if (tokens.Count != 3) return null; // invalid syntax
            var key = tokens[0];
            var op = tokens[1];
            var value = tokens[2];

            Expression<Func<Patient, bool>> expr = null;

            switch (key)
            {
                case "name":
                    if (op != "=") return null;
                    expr = (Patient p) => p.Name.ToLower().StartsWith(value) || p.Name.ToLower().Contains(value);
                    break;
                case "id":
                    if (op != "=") return null;
                    expr = (Patient p) => p.Id.ToLower().StartsWith(value);
                    break;
                case "hn":
                    if (op != "=") return null;
                    expr = (Patient p) => p.HospitalNumber.ToLower().StartsWith(value);
                    break;
                case "sex":
                case "gender":
                    if (op != "=") return null;
                    bool male = value[..1] == "m";
                    bool female = value[..1] == "f";
                    bool unknown = value[..1] == "u";

                    expr = (Patient p) => (p.Gender.ToLower().Substring(0, 1) == "m" == male && !female && !unknown) ||
                                        (p.Gender.ToLower().Substring(0, 1) == "f" == female && !male && !unknown) ||
                                        (string.IsNullOrEmpty(p.Gender) && unknown);
                    break;
                case "age":
                    int age;
                    if (int.TryParse(value, out age))
                    {
                        if (op == "=")
                        {
                            DateOnly lowerBound = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-age - 1));
                            DateOnly upperBound = lowerBound.AddYears(1);
                            expr = (Patient p) => p.BirthDate > lowerBound && p.BirthDate < upperBound;
                            break;
                        }
                        if (op == ">")
                        {
                            var datetimeTarget = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-age - 1));
                            expr = (Patient p) => p.BirthDate < datetimeTarget;
                            break;
                        }
                        if (op == "<")
                        {
                            var datetimeTarget = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-age));
                            expr = (Patient p) => p.BirthDate > datetimeTarget;
                        }
                    }
                    break;
                case "coverage":
                    if (op != "=") return null;
                    if (Enum.TryParse(value.Replace(" ", ""), true, out CoverageSchemeType converted))
                    {
                        expr = (Patient p) => p.CoverageScheme == converted;
                    }
                    else
                    {
                        if (value.StartsWith("c", StringComparison.InvariantCultureIgnoreCase))
                        {
                            expr = (Patient p) => p.CoverageScheme == CoverageSchemeType.Cash;
                        }
                        else if (value.StartsWith("g", StringComparison.InvariantCultureIgnoreCase))
                        {
                            expr = (Patient p) => p.CoverageScheme == CoverageSchemeType.Government;
                        }
                        else if (value.StartsWith("n", StringComparison.InvariantCultureIgnoreCase))
                        {
                            expr = (Patient p) => p.CoverageScheme == CoverageSchemeType.NationalHealthSecurity;
                        }
                        else if (value.StartsWith("s", StringComparison.InvariantCultureIgnoreCase))
                        {
                            expr = (Patient p) => p.CoverageScheme == CoverageSchemeType.SocialSecurity;
                        }
                        else if (value.StartsWith("o", StringComparison.InvariantCultureIgnoreCase))
                        {
                            expr = (Patient p) => p.CoverageScheme == CoverageSchemeType.Other;
                        }
                    }

                    break;
                default:
                    break;
            }

            if (expr == null)
            {
                return AdditionalSearchUsingToken(key, op, value);
            }

            return (Expression<Func<T, bool>>)replacer.Visit(expr);
        }

        protected override Expression<Func<T, bool>> ParseDefault(string whereString)
        {
            string searchTxt = whereString.ToLower();
            Expression<Func<Patient, bool>> expr = (Patient p) => p.Id.ToLower().StartsWith(searchTxt) ||
                            p.HospitalNumber.ToLower().StartsWith(searchTxt) ||
                            p.Name.ToLower().Contains(searchTxt);
            if (!Enum.TryParse(whereString.Replace(" ", ""), true, out CoverageSchemeType converted))
            {
                if (searchTxt == "national" || searchTxt == "nation")
                {
                    expr = expr.OrElse((Patient p) => p.CoverageScheme == CoverageSchemeType.NationalHealthSecurity);
                }
                else if (searchTxt == "social")
                {
                    expr = expr.OrElse((Patient p) => p.CoverageScheme == CoverageSchemeType.SocialSecurity);
                }
                else if (searchTxt == "gov" || searchTxt == "govern")
                {
                    expr = expr.OrElse((Patient p) => p.CoverageScheme == CoverageSchemeType.Government);
                }
                else if (searchTxt == "other")
                {
                    expr = expr.OrElse((Patient p) => p.CoverageScheme == CoverageSchemeType.Other);
                }
            }
            else
            {
                expr = expr.OrElse((Patient p) => p.CoverageScheme == converted);
            }

            return (Expression<Func<T, bool>>)replacer.Visit(expr);
        }

        protected virtual string[] AdditionalOperator() { return Array.Empty<string>(); }
        protected virtual Expression<Func<T, bool>> AdditionalSearchUsingToken(string key, string op, string value) { return null; }
    }
}
