using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class TagViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public string StrikeThroughStyle { get; set; }
    }
}
