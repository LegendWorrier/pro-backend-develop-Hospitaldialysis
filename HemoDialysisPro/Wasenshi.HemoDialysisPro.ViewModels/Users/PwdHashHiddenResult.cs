using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels.Users
{
    public class PwdHashHiddenResult
    {
        public UserPwdHashHidden User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
