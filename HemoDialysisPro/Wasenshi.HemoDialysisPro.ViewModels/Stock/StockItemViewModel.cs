using System;
using Wasenshi.HemoDialysisPro.Models.Entity.Accounting;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class StockItemViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public int ItemId { get; set; }
        public int UnitId { get; set; }

        public DateTimeOffset EntryDate { get; set; }
        public int Quantity { get; set; }
        public bool IsCredit { get; set; } // Credit means decrease for Assets (Stock is asset)
        public double PricePerPiece { get; set; }
        public StockType StockType { get; set; }
    }

    public class StockItemWithTypeViewModel : StockItemViewModel
    {
        public string Type { get; set; }
    }
}
