using System;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ShiftSlotViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public Guid UserId { get; set; }
        public long? ShiftMetaId { get; set; }
        public int? UnitId { get; set; } // flatten from shift meta
        public ShiftData ShiftData { get; set; }
    }
}
