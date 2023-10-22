using System;

namespace Wasenshi.HemoDialysisPro.Models.Interfaces
{
    public interface IUserRole
    {
        Guid UserId { get; set; }
        Guid RoleId { get; set; }
    }
}
