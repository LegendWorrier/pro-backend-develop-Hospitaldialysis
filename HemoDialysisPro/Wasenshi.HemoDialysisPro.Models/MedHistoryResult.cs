using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class MedHistoryResult
    {
        public Patient Patient { get; set; }
        public IEnumerable<DateTime> Columns { get; set; }
        public IEnumerable<KeyValuePair<Medicine, List<MedHistoryItem>[]>> Data { get; set; }
    }
}