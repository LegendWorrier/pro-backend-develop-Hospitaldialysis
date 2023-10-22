using System;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class ShiftSlot : EntityBase<Guid>
    {
        public DateOnly Date { get; set; }
        public Guid UserId { get; set; }

        public long? ShiftMetaId { get; set; }
        [Column(TypeName = "smallint")]
        public ShiftData Data { get; set; }

        [NotMapped]
        public ShiftMeta ShiftMeta { get; set; }

    }
}
