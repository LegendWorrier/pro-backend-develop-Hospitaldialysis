using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    [Owned]
    public class AVShuntRecord
    {
        public Guid? AVShuntId { get; set; }
        public string ShuntSite { get; set; }

        // ============= Non-AV ============================
        public float? ALength { get; set; } // cm
        public float? VLength { get; set; } // cm

        public float? ANeedleCC { get; set; }
        public float? VNeedleCC { get; set; }

        // ================= AV ====================
        public int? ASize { get; set; }
        public int? VSize { get; set; }

        public short? ANeedleTimes { get; set; }
        public short? VNeedleTimes { get; set; }


        [NotMapped]
        public AVShunt AVShunt { get; set; }
    }
}