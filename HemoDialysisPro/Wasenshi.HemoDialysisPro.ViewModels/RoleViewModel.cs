using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class RoleViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IList<string> Permissions { get; set; }
    }
}
