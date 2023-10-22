using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Entity.Accounting;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models.Entity.Stockable
{
    public class StockItemBase : EntityBase<Guid>
    {
        
        [Required]
        public int UnitId { get; set; }
        public DateTime EntryDate { get; set; }
        public int Quantity { get; set; }
        public bool IsCredit { get; set; } // Credit means decrease for Assets (Stock is asset)
        public double PricePerPiece { get; set; }
        public StockType StockType { get; set; }

        [NotMapped]
        public int Sum { get; set; }
    }

    public abstract class StockItem<T> : StockItemBase where T : Stockable
    {
        [Required]
        public int ItemId { get; set; }
        [NotMapped]
        public T ItemInfo { get; set; }
    }

    public class StockItem : StockItemBase
    {
        [Required]
        public int ItemId { get; set; }
        [NotMapped]
        public Stockable ItemInfo { get; set; }

        public string StockableType { get; set; }
    }
}
