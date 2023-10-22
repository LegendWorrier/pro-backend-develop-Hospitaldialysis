using System;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models.Entity.Accounting;
using System.Linq;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class StockItemSearch<TItem, TStock> : StockBaseSearcher<TItem> where TItem : StockItem<TStock> where TStock : Stockable
    {
        private readonly TimeZoneInfo tz;

        public StockItemSearch(TimeZoneInfo tz) : base(x => x.ItemInfo)
        {
            this.tz = tz;
        }

        protected override Expression<Func<TItem, bool>> ExtraDefaultSearch(string whereString)
        {
            if (DateTime.TryParse(whereString, out DateTime target))
            {
                var dTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(target, TimeSpan.Zero), tz);
                var lowerBound = dTz.ToUtcDate();
                var upperBound = lowerBound.AddDays(1);
                return (TItem s) => s.EntryDate >= lowerBound && s.EntryDate < upperBound;
            }
            if (Enum.TryParse(typeof(StockType), whereString, true, out object modeValue))
            {
                return (TItem s) => s.StockType == (StockType)modeValue;
            }

            return null;
        }

        protected override Expression<Func<TItem, bool>> AdditionalSearchUsingToken(string key, string op, string value)
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
                            return (TItem s) => s.EntryDate >= lowerBound && s.EntryDate < upperBound;
                        }
                        if (op == "<")
                        {
                            return (TItem s) => s.EntryDate < lowerBound;
                        }
                        if (op == ">")
                        {
                            return (TItem s) => s.EntryDate >= upperBound;
                        }
                    }
                    break;
                case "type":
                    if (op != "=") return null;
                    if (Enum.TryParse(typeof(StockType), value, true, out object modeValue))
                    {
                        return (TItem s) => s.StockType == (StockType)modeValue;
                    }
                    break;
                case "itemid":
                    if (op != "=") return null;
                    if (!int.TryParse(value, out int valueAsInt)) return null;
                    return (TItem s) => s.ItemId == valueAsInt;
            }

            return null;
        }

    }
}
