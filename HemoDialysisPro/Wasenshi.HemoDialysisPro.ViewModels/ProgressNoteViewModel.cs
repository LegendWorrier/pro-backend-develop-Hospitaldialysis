using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ProgressNoteViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public short Order { get; set; }

        public Guid HemodialysisId { get; set; }

        public string Focus { get; set; }

        public string A { get; set; } // Assessment
        public string I { get; set; } // Intervention
        public string E { get; set; } // Evaluation
    }
}