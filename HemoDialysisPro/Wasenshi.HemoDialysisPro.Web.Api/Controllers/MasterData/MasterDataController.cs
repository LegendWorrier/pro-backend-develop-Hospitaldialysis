using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Share.GlobalEvents;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers;
using System.Linq.Expressions;
using System;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [PermissionAuthorize(Permissions.MasterData.GENERAL)]
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly IMasterDataService _masterDataService;
        private readonly IMapper _mapper;
        private readonly IHubContext<UserHub, IUserClient> userHub;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;

        public MasterDataController(
            IMasterDataService masterDataService,
            IMapper mapper,
            IHubContext<UserHub, IUserClient> userHub,
            IRedisClient redis,
            IMessageQueueClient message)
        {
            _masterDataService = masterDataService;
            _mapper = mapper;
            this.userHub = userHub;
            this.redis = redis;
            this.message = message;
        }

        // ==================== Stock ========================
        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("stockable")]
        public IActionResult GetStockableList(string where = null)
        {
            Expression<Func<Stockable, bool>> condition = null;
            var searcher = new StockSearcher<Stockable>();
            if (where != null)
            {
                condition = searcher.GetWhereCondition(where);
            }

            var list = _masterDataService.GetStockableList(condition);

            return Ok(_mapper.Map<IEnumerable<StockableWithTypeViewModel>>(list));
        }

        // =================== Patient History ===================

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Patient-history")]
        public IActionResult GetPatientHistoryList()
        {
            var list = _masterDataService.GetPatientHistoryList();

            return Ok(_mapper.Map<IEnumerable<PatientHistoryItemViewModel>>(list));
        }

        [PermissionAuthorize(Permissions.MasterData.PATIENT_HISTORY)]
        [HttpPost("Patient-history")]
        public IActionResult AddPatientHistoryItem([FromBody] PatientHistoryItemViewModel item)
        {
            PatientHistoryItem newItem = _mapper.Map<PatientHistoryItem>(item);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<PatientHistoryItemViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.PATIENT_HISTORY)]
        [HttpPost("Patient-history/{id}")]
        public IActionResult EditPatientHistoryItem(int id, [FromBody] PatientHistoryItemViewModel item)
        {
            PatientHistoryItem update = _mapper.Map<PatientHistoryItem>(item);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Patient History entry not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.PATIENT_HISTORY)]
        [HttpDelete("Patient-history/{id}")]
        public IActionResult DeletePatientHistoryItem(int id)
        {
            var result = _masterDataService.RemoveMasterData<PatientHistoryItem>(id);

            if (!result)
            {
                return NotFound("Patient History not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.PATIENT_HISTORY)]
        [HttpPut("Patient-history/swap/{firstId}/{secondId}")]
        public IActionResult SwapOrderForPatientHistoryItem(int firstId, int secondId)
        {
            var result = _masterDataService.SwapOrderPatientHistoryItems(firstId, secondId);

            if (!result)
            {
                return NotFound("Patient History not found.");
            }

            return Ok();
        }

        // ===================================================

        [PermissionAuthorize(Permissions.MasterData.UNIT)]
        [HttpGet("Unit/Refresh")]
        public IActionResult RefreshUnitCache()
        {
            redis.As<Unit>().StoreAll(_masterDataService.GetMasterDataList<Unit>().Take(LicenseManager.MaxUnits));
            return Ok();
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Unit")]
        public IActionResult GetUnitList()
        {
            var list = _masterDataService.GetMasterDataList<Unit>().OrderBy(x => x.Id).Take(LicenseManager.MaxUnits);

            return Ok(_mapper.Map<IEnumerable<UnitViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Medicine")]
        public IActionResult GetMedicineList()
        {
            var list = _masterDataService.GetMasterDataList<Medicine>();

            return Ok(_mapper.Map<IEnumerable<MedicineViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("MedicineCategory")]
        public IActionResult GetMedicineCategoryList()
        {
            var list = _masterDataService.GetMasterDataList<MedCategory>();

            return Ok(_mapper.Map<IEnumerable<MasterDataViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Status")]
        public IActionResult GetStatusList()
        {
            var list = _masterDataService.GetMasterDataList<Status>();

            return Ok(_mapper.Map<IEnumerable<StatusViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("DeathCause")]
        public IActionResult GetDeathCauseList()
        {
            var list = _masterDataService.GetMasterDataList<DeathCause>();

            return Ok(_mapper.Map<IEnumerable<MasterDataViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("AC")]
        public IActionResult GetAnticoagulantList()
        {
            var list = _masterDataService.GetMasterDataList<Anticoagulant>();

            return Ok(_mapper.Map<IEnumerable<MasterDataViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Dialyzer")]
        public IActionResult GetDialyzerList()
        {
            var list = _masterDataService.GetMasterDataList<Dialyzer>();

            return Ok(_mapper.Map<IEnumerable<DialyzerViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Dialysate")]
        public IActionResult GetDialysateList()
        {
            var list = _masterDataService.GetMasterDataList<Dialysate>();

            return Ok(_mapper.Map<IEnumerable<DialysateViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Needle")]
        public IActionResult GetNeedleList()
        {
            var list = _masterDataService.GetMasterDataList<Needle>();

            return Ok(_mapper.Map<IEnumerable<NeedleViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Underlying")]
        public IActionResult GetUnderlyingList()
        {
            var list = _masterDataService.GetMasterDataList<Underlying>();

            return Ok(_mapper.Map<IEnumerable<MasterDataViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("LabItem")]
        public IActionResult GetLabItemList()
        {
            var list = _masterDataService.GetLabExamItemList();

            return Ok(_mapper.Map<IEnumerable<LabExamItemViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Ward")]
        public IActionResult GetWardList()
        {
            var list = _masterDataService.GetMasterDataList<Ward>();

            return Ok(_mapper.Map<IEnumerable<WardViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("MedSupply")]
        public IActionResult GetMedSupplyList()
        {
            var list = _masterDataService.GetMasterDataList<MedicalSupply>();

            return Ok(_mapper.Map<IEnumerable<StockableViewModel>>(list));
        }

        [BypassAuthorize]
        [AllowAnonymous]
        [HttpGet("Equipment")]
        public IActionResult GetEquipmentList()
        {
            var list = _masterDataService.GetMasterDataList<Equipment>();

            return Ok(_mapper.Map<IEnumerable<StockableViewModel>>(list));
        }

        // ========= Add ============================

        [PermissionAuthorize(Permissions.MasterData.UNIT)]
        [HttpPost("Unit")]
        public IActionResult AddUnit([FromBody] UnitViewModel unit)
        {
            if (_masterDataService.CheckMaxUnits())
            {
                throw new AppException("MAX_UNIT", "Maximum number of units reached. Upgrade your license to increase limit.");
            }

            Unit newItem = _mapper.Map<Unit>(unit);

            var result = _masterDataService.AddMasterData(newItem);

            UpdateHemoBoxMeta(result);

            return Created($"{result.Id}", _mapper.Map<UnitViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.MEDICINE)]
        [HttpPost("Medicine")]
        public IActionResult AddMedicine([FromBody] MedicineViewModel medicine)
        {
            Medicine newItem = _mapper.Map<Medicine>(medicine);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<MedicineViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.MED_CATEGORY)]
        [HttpPost("MedicineCategory")]
        public IActionResult AddMedicineCategory([FromBody] MasterDataViewModel category)
        {
            MedCategory newItem = _mapper.Map<MedCategory>(category);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<MasterDataViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.STATUS)]
        [HttpPost("Status")]
        public IActionResult AddStatus([FromBody] StatusViewModel status)
        {
            Status newItem = _mapper.Map<Status>(status);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<StatusViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.DEATHCAUSE)]
        [HttpPost("DeathCause")]
        public IActionResult AddDeathCause([FromBody] MasterDataViewModel deathCause)
        {
            DeathCause newItem = _mapper.Map<DeathCause>(deathCause);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<MasterDataViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.ANTICOAGULANT)]
        [HttpPost("AC")]
        public IActionResult AddAC([FromBody] MasterDataViewModel ac)
        {
            Anticoagulant newItem = _mapper.Map<Anticoagulant>(ac);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<MasterDataViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.DIALYZER)]
        [HttpPost("Dialyzer")]
        public IActionResult AddDialyzer([FromBody] DialyzerViewModel dialyzer)
        {
            Dialyzer newItem = _mapper.Map<Dialyzer>(dialyzer);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<DialyzerViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.DIALYSATE)]
        [HttpPost("Dialysate")]
        public IActionResult AddDialysate([FromBody] DialysateViewModel dialysate)
        {
            Dialysate newItem = _mapper.Map<Dialysate>(dialysate);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<DialysateViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.NEEDLE)]
        [HttpPost("Needle")]
        public IActionResult AddNeedle([FromBody] NeedleViewModel needle)
        {
            Needle newItem = _mapper.Map<Needle>(needle);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<NeedleViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.UNDERLYING)]
        [HttpPost("Underlying")]
        public IActionResult AddUnderlying([FromBody] MasterDataViewModel underlying)
        {
            Underlying newItem = _mapper.Map<Underlying>(underlying);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<MasterDataViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.LAB)]
        [HttpPost("LabItem")]
        public IActionResult AddLabItem([FromBody] LabExamItemViewModel labItem)
        {
            LabExamItem newItem = _mapper.Map<LabExamItem>(labItem);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<LabExamItemViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.WARD)]
        [HttpPost("Ward")]
        public IActionResult AddWard([FromBody] WardViewModel ward)
        {
            Ward newItem = _mapper.Map<Ward>(ward);

            var result = _masterDataService.AddMasterData(newItem);

            return Created($"{result.Id}", _mapper.Map<WardViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.MEDSUPPLY)]
        [HttpPost("MedSupply")]
        public IActionResult AddMedicalSupply([FromBody] StockableViewModel medSupply)
        {
            MedicalSupply newItem = _mapper.Map<MedicalSupply>(medSupply);

            var result = _masterDataService.AddMasterData<MedicalSupply>(newItem);

            return Created($"{result.Id}", _mapper.Map<StockableViewModel>(result));
        }

        [PermissionAuthorize(Permissions.MasterData.EQUIPMENT)]
        [HttpPost("Equipment")]
        public IActionResult AddEquipment([FromBody] StockableViewModel equipment)
        {
            Equipment newItem = _mapper.Map<Equipment>(equipment);

            var result = _masterDataService.AddMasterData<Equipment>(newItem);

            return Created($"{result.Id}", _mapper.Map<StockableViewModel>(result));
        }

        // =========== edit ===============
        [PermissionAuthorize(Permissions.MasterData.UNIT)]
        [HttpPost("Unit/{id}")]
        public IActionResult EditUnit(int id, UnitViewModel unit)
        {
            Unit update = _mapper.Map<Unit>(unit);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Unit not found.");
            }

            UpdateHemoBoxMeta(update);

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.MEDICINE)]
        [HttpPost("Medicine/{id}")]
        public IActionResult EditMedicine(int id, MedicineViewModel medicine)
        {
            Medicine update = _mapper.Map<Medicine>(medicine);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData<Medicine>(update);

            if (!result)
            {
                return NotFound("Medicine not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.MED_CATEGORY)]
        [HttpPost("MedicineCategory/{id}")]
        public IActionResult EditMedicineCategory(int id, MasterDataViewModel category)
        {
            MedCategory update = _mapper.Map<MedCategory>(category);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Medicine Category not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.STATUS)]
        [HttpPost("Status/{id}")]
        public IActionResult EditStatus(int id, StatusViewModel status)
        {
            Status update = _mapper.Map<Status>(status);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Status not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.DEATHCAUSE)]
        [HttpPost("DeathCause/{id}")]
        public IActionResult EditDeathCause(int id, MasterDataViewModel deathCause)
        {
            DeathCause update = _mapper.Map<DeathCause>(deathCause);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("DeathCause not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.ANTICOAGULANT)]
        [HttpPost("AC/{id}")]
        public IActionResult EditAC(int id, MasterDataViewModel ac)
        {
            Anticoagulant update = _mapper.Map<Anticoagulant>(ac);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Anticoagulant not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.DIALYZER)]
        [HttpPost("Dialyzer/{id}")]
        public IActionResult EditDialyzer(int id, DialyzerViewModel dialyzer)
        {
            Dialyzer update = _mapper.Map<Dialyzer>(dialyzer);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Dialyzer not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.DIALYSATE)]
        [HttpPost("Dialysate/{id}")]
        public IActionResult EditDialysate(int id, DialysateViewModel dialysate)
        {
            Dialysate update = _mapper.Map<Dialysate>(dialysate);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Dialysate not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.NEEDLE)]
        [HttpPost("Needle/{id}")]
        public IActionResult EditNeedle(int id, NeedleViewModel needle)
        {
            Needle update = _mapper.Map<Needle>(needle);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Needle not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.UNDERLYING)]
        [HttpPost("Underlying/{id}")]
        public IActionResult EditUnderlying(int id, MasterDataViewModel underlying)
        {
            Underlying update = _mapper.Map<Underlying>(underlying);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Underlying not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.LAB)]
        [HttpPost("LabItem/{id}")]
        public IActionResult EditLabItem(int id, LabExamItemViewModel labItem)
        {
            LabExamItem update = _mapper.Map<LabExamItem>(labItem);
            update.Id = id;
            var result = _masterDataService.UpdateLabExamItem(update);

            if (!result)
            {
                return NotFound("Lab Exam Item not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.WARD)]
        [HttpPost("Ward/{id}")]
        public IActionResult EditWard(int id, WardViewModel ward)
        {
            Ward update = _mapper.Map<Ward>(ward);
            update.Id = id;
            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Ward not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.MEDSUPPLY)]
        [HttpPost("MedSupply/{id}")]
        public IActionResult EditMedicalSupply(int id, StockableViewModel medSupply)
        {
            MedicalSupply update = _mapper.Map<MedicalSupply>(medSupply);
            update.Id = id;

            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Med Supply not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.EQUIPMENT)]
        [HttpPost("Equipment/{id}")]
        public IActionResult EditEquipment(int id, StockableViewModel equipment)
        {
            Equipment update = _mapper.Map<Equipment>(equipment);
            update.Id = id;

            var result = _masterDataService.UpdateMasterData(update);

            if (!result)
            {
                return NotFound("Equipment not found.");
            }

            return Ok();
        }

        // ================== delete ======================

        [PermissionAuthorize(Permissions.MasterData.UNIT)]
        [HttpDelete("Unit/{id}")]
        public IActionResult DeleteUnit(int id)
        {
            // Safe-guard : cannot delete if there is only 1 unit left
            if (_masterDataService.GetMasterDataList<Unit>().Count() == 1)
            {
                return BadRequest("Cannot delete the only unit left.");
            }

            var result = _masterDataService.RemoveMasterData<Unit>(id);

            if (!result)
            {
                return NotFound("Unit not found.");
            }

            UpdateHemoBoxMeta(new Unit { Id = id }, true);

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.MEDICINE)]
        [HttpDelete("Medicine/{id}")]
        public IActionResult DeleteMedicine(int id)
        {
            var result = _masterDataService.RemoveMasterData<Medicine>(id);

            if (!result)
            {
                return NotFound("Medicine not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.MED_CATEGORY)]
        [HttpDelete("MedicineCategory/{id}")]
        public IActionResult DeleteMedicineCategory(int id)
        {
            var result = _masterDataService.RemoveMasterData<MedCategory>(id);

            if (!result)
            {
                return NotFound("Medicine Category not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.STATUS)]
        [HttpDelete("Status/{id}")]
        public IActionResult DeleteStatus(int id)
        {
            var result = _masterDataService.RemoveMasterData<Status>(id);

            if (!result)
            {
                return NotFound("Status not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.DEATHCAUSE)]
        [HttpDelete("DeathCause/{id}")]
        public IActionResult DeleteDeathCause(int id)
        {
            var result = _masterDataService.RemoveMasterData<DeathCause>(id);

            if (!result)
            {
                return NotFound("DeathCause not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.ANTICOAGULANT)]
        [HttpDelete("AC/{id}")]
        public IActionResult DeleteAC(int id)
        {
            var result = _masterDataService.RemoveMasterData<Anticoagulant>(id);

            if (!result)
            {
                return NotFound("Anticoagulant not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.DIALYZER)]
        [HttpDelete("Dialyzer/{id}")]
        public IActionResult DeleteDialyzer(int id)
        {
            var result = _masterDataService.RemoveMasterData<Dialyzer>(id);

            if (!result)
            {
                return NotFound("Dialyzer not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.DIALYSATE)]
        [HttpDelete("Dialysate/{id}")]
        public IActionResult DeleteDialysate(int id)
        {
            var result = _masterDataService.RemoveMasterData<Dialysate>(id);

            if (!result)
            {
                return NotFound("Dialysate not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.NEEDLE)]
        [HttpDelete("Needle/{id}")]
        public IActionResult DeleteNeedle(int id)
        {
            var result = _masterDataService.RemoveMasterData<Needle>(id);

            if (!result)
            {
                return NotFound("Needle not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.UNDERLYING)]
        [HttpDelete("Underlying/{id}")]
        public IActionResult DeleteUnderlying(int id)
        {
            var result = _masterDataService.RemoveMasterData<Underlying>(id);

            if (!result)
            {
                return NotFound("Underlying not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.LAB)]
        [HttpDelete("LabItem/{id}")]
        public IActionResult DeleteLabItem(int id)
        {
            LabExamItem lab = _masterDataService.GetLabExamItem(id);
            if (lab == null)
            {
                return NotFound("Lab Exam Item not found.");
            }

            if (lab.IsSystemBound)
            {
                return BadRequest();
            }

            _masterDataService.RemoveMasterData<LabExamItem>(id);

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.WARD)]
        [HttpDelete("Ward/{id}")]
        public IActionResult DeleteWard(int id)
        {
            var result = _masterDataService.RemoveMasterData<Ward>(id);

            if (!result)
            {
                return NotFound("Ward not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.MEDSUPPLY)]
        [HttpDelete("MedSupply/{id}")]
        public IActionResult DeleteMedicalSupply(int id)
        {
            var result = _masterDataService.RemoveMasterData<MedicalSupply>(id);

            if (!result)
            {
                return NotFound("Med Supply not found.");
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.MasterData.EQUIPMENT)]
        [HttpDelete("Equipment/{id}")]
        public IActionResult DeleteEquipment(int id)
        {
            var result = _masterDataService.RemoveMasterData<Equipment>(id);

            if (!result)
            {
                return NotFound("Equipment not found.");
            }

            return Ok();
        }

        // =============== Utils ================================

        private void UpdateHemoBoxMeta(Unit unit, bool remove = false)
        {
            HemoBoxQueue.AddWorkToQueue(async (scope) =>
            {
                var hemoboxHub = scope.ServiceProvider.GetRequiredService<IHubContext<HemoBoxHub, IHemoBoxClient>>();
                await _UpdateUnitMetaEvent(hemoboxHub, unit, remove);
            });
            HemoBoxQueue.StartImmediately();

            message.Publish(new UnitUpdated { Data = unit, Remove = remove });

            // TODO: notify all connected user clients that unit list meta has been modified ?
        }

        private static async Task _UpdateUnitMetaEvent(IHubContext<HemoBoxHub, IHemoBoxClient> hemoboxHub, Unit unit, bool remove = false)
        {
            UnitInfo unitInfo = new UnitInfo
            {
                Id = unit.Id,
                Name = unit.Name
            };
            await hemoboxHub.Clients.All.UnitMetaUpdate(unitInfo, remove);
        }
    }
}
