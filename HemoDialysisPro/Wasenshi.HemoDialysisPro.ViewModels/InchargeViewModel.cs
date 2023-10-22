using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class InchargeViewModel : EntityViewModel
    {
        public int UnitId { get; set; }
        public DateOnly Date { get; set; }
        public Guid? UserId { get; set; }

        public IEnumerable<InchargeSectionViewModel> Sections { get; set; }
    }

    public class InchargeSectionViewModel
    {
        public int SectionId { get; set; }
        public Guid UserId { get; set; }
    }
}
