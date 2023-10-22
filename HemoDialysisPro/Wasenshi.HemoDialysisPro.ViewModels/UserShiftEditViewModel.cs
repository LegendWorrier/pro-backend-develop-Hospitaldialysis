using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class UserShiftEditViewModel : EntityViewModel
    {
        public long Id { get; set; }
        public DateOnly Month { get; set; }
        public Guid UserId { get; set; }
        public bool Suspended { get; set; }
    }
}
