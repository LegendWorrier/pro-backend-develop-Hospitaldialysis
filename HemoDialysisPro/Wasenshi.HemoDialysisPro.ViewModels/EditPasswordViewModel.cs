using System;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class EditPasswordViewModel
    {
        public Guid Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
