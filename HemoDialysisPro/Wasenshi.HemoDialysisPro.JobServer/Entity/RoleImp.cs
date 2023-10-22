using System.ComponentModel.DataAnnotations;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.JobsServer
{
    public class RoleImp : IRole
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
