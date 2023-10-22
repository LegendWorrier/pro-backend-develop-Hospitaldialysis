using Microsoft.AspNetCore.Identity;
using System;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models.Infrastructor
{
    public class UserRole : IdentityUserRole<Guid>, IUserRole
    {
    }
}
