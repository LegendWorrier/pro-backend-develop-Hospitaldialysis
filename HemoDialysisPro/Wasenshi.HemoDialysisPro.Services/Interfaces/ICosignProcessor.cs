using System;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface ICosignProcessor : IApplicationService
    {
        Task<bool> ValidateCosignAsync<T>(Guid cosignUserId, string cosignPassword, T resource) where T : EntityBase;
    }
}
