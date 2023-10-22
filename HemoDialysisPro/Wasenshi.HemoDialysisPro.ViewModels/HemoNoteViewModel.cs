using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class HemoNoteViewModel : EntityViewModel
    {
        public Guid? Id { get; set; }
        public Guid HemoId { get; set; }
        public string Complication { get; set; }
    }
}
