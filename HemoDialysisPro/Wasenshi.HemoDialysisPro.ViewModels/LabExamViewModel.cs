using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class LabExamViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public DateTimeOffset EntryTime { get; set; }
        public int LabItemId { get; set; }
        public float LabValue { get; set; }

        public string Note { get; set; }

        public LabExamItemViewModel LabItem { get; set; }
    }
}
