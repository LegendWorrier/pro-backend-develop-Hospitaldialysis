using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models.Entity.Stockable
{
    public abstract class Stockable : EntityBase<int>, IStockable
    {
        [Required]
        public string Name { get;set; }
        public string Code { get; set; }
        public string PieceUnit { get; set; }

        public string Barcode { get; set; }
        public string Note { get; set; }

        public string Image { get; set; }
    }
}
