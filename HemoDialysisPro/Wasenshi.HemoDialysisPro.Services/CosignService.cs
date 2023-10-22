using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class CosignService : ICosignService
    {
        private readonly ICosignProcessor cosignProcessor;
        private readonly IUserInfoService userInfoService;
        private readonly IHemoService hemoService;
        private readonly IHemoUnitOfWork hemoUOW;
        private readonly IExecutionRecordRepository executionRecordRepo;

        public CosignService(
            ICosignProcessor cosignProcessor,
            IUserInfoService userInfoService,
            IHemoService hemoService,
            IHemoUnitOfWork hemoUOW,
            IExecutionRecordRepository executionRecordRepo)
        {
            this.cosignProcessor = cosignProcessor;
            this.userInfoService = userInfoService;
            this.hemoService = hemoService;
            this.hemoUOW = hemoUOW;
            this.executionRecordRepo = executionRecordRepo;
        }

        public async Task<bool> AssignCosignForHemosheet(Guid recordId, Guid cosignUserId, string cosignPassword)
        {
            return await _AssignCosignForHemosheet(recordId, cosignUserId, cosignPassword);
        }

        public async Task<bool> AssignCosignForHemosheet(Guid recordId, Guid cosignUserId)
        {
            return await _AssignCosignForHemosheet(recordId, cosignUserId);
        }

        private async Task<bool> _AssignCosignForHemosheet(Guid recordId, Guid cosignUserId, string cosignPassword = null)
        {
            var hemoRecord = hemoService.GetHemodialysisRecord(recordId);
            if (!string.IsNullOrWhiteSpace(cosignPassword))
            {
                var validate = await cosignProcessor.ValidateCosignAsync(cosignUserId, cosignPassword, hemoRecord);
                if (!validate)
                {
                    return false;
                }
            }
            // update hemo record
            hemoRecord.ProofReader = cosignUserId;
            var entity = hemoUOW.HemoRecord.Update(hemoRecord);

            var prescription = entity.Reference(x => x.DialysisPrescription);
            if (prescription.TargetEntry != null)
            {
                prescription.TargetEntry.State = EntityState.Unchanged;
            }

            return hemoUOW.Complete() > 0;
        }

        public async Task<bool> AssignCosignForExecutionRecord(Guid id, Guid cosignUserId, string cosignPassword)
        {
            return await _AssignCosignForExecutionRecord(id, cosignUserId, cosignPassword);
        }

        public async Task<bool> AssignCosignForExecutionRecord(Guid id, Guid cosignUserId)
        {
            return await _AssignCosignForExecutionRecord(id, cosignUserId);
        }

        private async Task<bool> _AssignCosignForExecutionRecord(Guid id, Guid cosignUserId, string cosignPassword = null)
        {
            var executionRecord = executionRecordRepo.Get(id);
            if (!string.IsNullOrWhiteSpace(cosignPassword))
            {
                bool validate = await cosignProcessor.ValidateCosignAsync(cosignUserId, cosignPassword, executionRecord);
                if (!validate)
                {
                    return false;
                }
            }

            // update execution record
            executionRecord.CoSign = cosignUserId;
            executionRecordRepo.Update(executionRecord);

            return executionRecordRepo.Complete() > 0;
        }

        public async Task<bool> AssignNurseForDialysisPrescription(Guid id, Guid nurseId, string cosignPassword)
        {
            return await _AssignNurseForDialysisPrescription(id, nurseId, cosignPassword);
        }

        public async Task<bool> AssignNurseForDialysisPrescription(Guid id, Guid nurseId)
        {
            return await _AssignNurseForDialysisPrescription(id, nurseId);
        }

        private async Task<bool> _AssignNurseForDialysisPrescription(Guid id, Guid nurseId, string cosignPassword = null)
        {
            // validate nurse
            var user = userInfoService.FindUser(x => x.Id == nurseId);
            if (user == null || user.Roles.Contains(Roles.Doctor)) // anyone but a doctor
            {
                return false;
            }

            var prescription = hemoUOW.Prescription.Get(id);
            if (!string.IsNullOrWhiteSpace(cosignPassword))
            {
                bool validate = await cosignProcessor.ValidateCosignAsync(nurseId, cosignPassword, prescription);
                if (!validate)
                {
                    return false;
                }
            }

            // update execution record
            prescription.DialysisNurse = nurseId;
            hemoUOW.Prescription.Update(prescription);

            return hemoUOW.Complete() > 0;
        }
    }
}
