using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class DoctorRecordViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public Guid HemodialysisId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Content { get; set; }
    }
}
