using System;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class StockBaseSearcher<T> : ModelSearcher<T>
    {
        private readonly ModelReplacer<Stockable> replacer;

        public StockBaseSearcher(Expression<Func<T, Stockable>> getStock)
        {
            replacer = new ModelReplacer<Stockable>(getStock);
        }

        protected override Expression<Func<T, bool>> ParseConditionBlock(string whereString)
        {
            var tokens = Tokenize(whereString, false, new[] { "=", "<", ">" }.Concat(AdditionalOperator()).ToArray()).ToList();
            if (tokens.Count != 3) return null; // invalid syntax
            var key = tokens[0];
            var op = tokens[1];
            var value = tokens[2];

            Expression<Func<Stockable, bool>> expr = null;

            switch (key)
            {
                case "name":
                    if (op != "=") return null;
                    expr = (Stockable s) => s.Name.ToLower().StartsWith(value) || s.Name.ToLower().Contains(value);
                    break;
                case "code":
                    if (op != "=") return null;
                    expr = (Stockable s) => s.Code.ToLower().StartsWith(value);
                    break;

                case "barcode":
                    if (op != "=") return null;
                    expr = (Stockable s) => s.Barcode.ToLower() == value;
                    break;
                case "id":
                    if (op != "=") return null;
                    if (int.TryParse(value, out int idValue))
                    {
                        expr = (Stockable s) => s.Id == idValue;
                    }
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
            Expression<Func<T, bool>> parsed = ExtraDefaultSearch(whereString);
            if (parsed != null) return parsed;

            string searchTxt = whereString.ToLower();
            Expression<Func<Stockable, bool>> expr = (Stockable s) => s.Name.ToLower().StartsWith(searchTxt) ||
                            s.Code.ToLower().StartsWith(searchTxt) ||
                            s.Barcode.ToLower().Contains(searchTxt);

            return (Expression<Func<T, bool>>)replacer.Visit(expr);
        }

        protected virtual string[] AdditionalOperator() { return Array.Empty<string>(); }
        protected virtual Expression<Func<T, bool>> AdditionalSearchUsingToken(string key, string op, string value) { return null; }
        protected virtual Expression<Func<T, bool>> ExtraDefaultSearch(string whereString) { return null; }
    }
}
