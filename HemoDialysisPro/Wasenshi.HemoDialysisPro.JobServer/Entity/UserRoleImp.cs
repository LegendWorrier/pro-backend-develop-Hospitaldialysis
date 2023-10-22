using System.ComponentModel.DataAnnotations;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.JobsServer
{
    public class UserRoleImp : IUserRole
    {
        [Key]
        public Guid UserId { get; set; }
        [Key]
        public Guid RoleId { get; set; }
    }
}
