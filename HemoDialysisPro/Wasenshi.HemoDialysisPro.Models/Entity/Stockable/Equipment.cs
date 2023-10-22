using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models.Entity.Stockable
{
    public class Equipment : Stockable
    {
    }

    public class EquipmentStock : StockItem<Equipment>
    {
    }
}
