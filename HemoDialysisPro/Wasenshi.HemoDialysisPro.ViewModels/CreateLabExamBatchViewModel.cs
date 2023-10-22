using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class CreateLabExamBatchViewModel : EntityViewModel
    {
        public string PatientId { get; set; }
        public DateTimeOffset EntryTime { get; set; }

        public IEnumerable<LabInfoViewModel> LabExams { get; set; }
    }

    public class LabInfoViewModel
    {
        public int LabItemId { get; set; }
        public float LabValue { get; set; }
    }
}
