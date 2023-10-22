using System;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class LabExamResult
    {
        public Patient Patient { get; set; }
        public IEnumerable<DateTime> Columns { get; set; }
        public IEnumerable<KeyValuePair<LabExamItem, List<LabExam>[]>> Data { get; set; }
    }
}