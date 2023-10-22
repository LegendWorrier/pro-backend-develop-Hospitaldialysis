using System;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class CompleteHemoViewModel
    {
        public EditHemodialysisRecordViewModel Update { get; set; }
        public DateTimeOffset? CompleteTime { get; set; }
    }
}
