using System;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface ICosignService : IApplicationService
    {
        Task<bool> AssignCosignForHemosheet(Guid recordId, Guid cosignUserId, string cosignPassword);
        Task<bool> AssignCosignForHemosheet(Guid recordId, Guid cosignUserId);
        Task<bool> AssignCosignForExecutionRecord(Guid id, Guid cosignUserId, string cosignPassword);
        Task<bool> AssignCosignForExecutionRecord(Guid id, Guid cosignUserId);

        Task<bool> AssignNurseForDialysisPrescription(Guid id, Guid cosignUserId, string cosignPassword);
        Task<bool> AssignNurseForDialysisPrescription(Guid id, Guid cosignUserId);
    }
}
