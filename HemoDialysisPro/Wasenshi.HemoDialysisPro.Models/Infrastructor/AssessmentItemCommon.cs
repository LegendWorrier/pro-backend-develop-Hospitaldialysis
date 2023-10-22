using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models.Infrastructor
{
    public class AssessmentItemCommon : EntityBase<Guid>
    {
        public long AssessmentId { get; set; }
        public long[] Selected { get; set; }
        public bool Checked { get; set; }
        public string Text { get; set; }
        public float? Value { get; set; }

        [NotMapped]
        public Assessment Assessment { get; set; }
    }
}
