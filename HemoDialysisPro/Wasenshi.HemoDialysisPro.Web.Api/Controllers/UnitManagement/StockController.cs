using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Services.Core.Stock;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers;
using Microsoft.AspNetCore.Authorization;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels.Stock;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Authorize(Policy = Feature.MANAGEMENT)]
    [PermissionAuthorize(Permissions.STOCK)]
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService stockService;
        private readonly IMapper mapper;
        private readonly IConfiguration config;

        private TimeZoneInfo tz;

        public StockController(
            IStockService stockService,
            IMapper mapper,
            IConfiguration config)
        {
            this.stockService = stockService;
            this.mapper = mapper;
            this.config = config;

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        // ========== Get Specific Stock Item ===========

        [HttpGet("{id}")]
        public IActionResult GetStockItemById(Guid id)
        {
            var result = stockService.GetStockItemById(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // ================ Add Lot ====================
        [HttpPost("add-lot")]
        public IActionResult AddStockLot(StockLotViewModel request)
        {
            List<StockItemBase> list = new List<StockItemBase>();
            foreach (var item in request.Data)
            {
                switch (item.Type.ToLower())
                {
                    case "med":
                        list.Add(mapper.Map<MedicineStock>(item));
                        break;
                    case "supply":
                        list.Add(mapper.Map<MedicalSupplyStock>(item));
                        break;
                    case "dialyzer":
                        list.Add(mapper.Map<DialyzerStock>(item));
                        break;
                    case "equipment":
                        list.Add(mapper.Map<EquipmentStock>(item));
                        break;
                }
            }

            var result = stockService.AddStockLot(list);
            if (!result)
            {
                return NotFound();
            }

            return Ok(list);
        }

        // ======== Overview =================

        [HttpGet("Medicine")]
        public IActionResult GetMedicineStockList(int pageSize = 10, int page = 1, int? unitId = null, [FromQuery] List<string> orderBy = null, string where = null)
        {
            Expression<Func<MedicineStock, bool>> whereCondition = null;
            if (where != null)
            {
                var searcher = new StockItemSearch<MedicineStock, Medicine>(tz);
                whereCondition = searcher.GetWhereCondition(where);
            }
            var result = unitId.HasValue ? stockService.GetStockOverviewForUnit(unitId.Value, page, pageSize, GetIntermidiateOrder<MedicineStock, Medicine>(orderBy), whereCondition) :
                stockService.GetStockOverview(page, pageSize, GetOverviewOrder<Medicine>(orderBy), whereCondition);

            return Ok(result);
        }

        [HttpGet("MedSupply")]
        public IActionResult GetMedSupplyStockList(int pageSize = 10, int page = 1, int? unitId = null, [FromQuery] List<string> orderBy = null, string where = null)
        {
            Expression<Func<MedicalSupplyStock, bool>> whereCondition = null;
            if (where != null)
            {
                var searcher = new StockItemSearch<MedicalSupplyStock, MedicalSupply>(tz);
                whereCondition = searcher.GetWhereCondition(where);
            }
            var result = unitId.HasValue ? stockService.GetStockOverviewForUnit(unitId.Value, page, pageSize, GetIntermidiateOrder<MedicalSupplyStock, MedicalSupply>(orderBy), whereCondition) :
                stockService.GetStockOverview(page, pageSize, GetOverviewOrder<MedicalSupply>(orderBy), whereCondition);

            return Ok(result);
        }

        [HttpGet("Equipment")]
        public IActionResult GetEquipmentStockList(int pageSize = 10, int page = 1, int? unitId = null, [FromQuery] List<string> orderBy = null, string where = null)
        {
            Expression<Func<EquipmentStock, bool>> whereCondition = null;
            if (where != null)
            {
                var searcher = new StockItemSearch<EquipmentStock, Equipment>(tz);
                whereCondition = searcher.GetWhereCondition(where);
            }
            var result = unitId.HasValue ? stockService.GetStockOverviewForUnit(unitId.Value, page, pageSize, GetIntermidiateOrder<EquipmentStock, Equipment>(orderBy), whereCondition) :
                stockService.GetStockOverview<EquipmentStock, Equipment>(page, pageSize, GetOverviewOrder<Equipment>(orderBy), whereCondition);

            return Ok(result);
        }

        [HttpGet("Dialyzer")]
        public IActionResult GetDialyzerStockList(int pageSize = 10, int page = 1, int? unitId = null, [FromQuery] List<string> orderBy = null, string where = null)
        {
            Expression<Func<DialyzerStock, bool>> whereCondition = null;
            if (where != null)
            {
                var searcher = new StockItemSearch<DialyzerStock, Dialyzer>(tz);
                whereCondition = searcher.GetWhereCondition(where);
            }
            var result = unitId.HasValue ? stockService.GetStockOverviewForUnit(unitId.Value, page, pageSize, GetIntermidiateOrder<DialyzerStock, Dialyzer>(orderBy), whereCondition) :
                stockService.GetStockOverview<DialyzerStock, Dialyzer>(page, pageSize, GetOverviewOrder<Dialyzer>(orderBy), whereCondition);

            return Ok(result);
        }


        // ========== Detail List ================

        [HttpGet("Medicine/{id}")]
        public IActionResult GetMedicineStockDetail(int id, int pageSize = 10, int page = 1, int? unitId = null, string where = null)
        {
            Expression<Func<MedicineStock, bool>> whereCondition = x => x.ItemId == id;
            if (where != null)
            {
                var searcher = new StockItemSearch<MedicineStock, Medicine>(tz);
                whereCondition = whereCondition.AndAlso(searcher.GetWhereCondition(where));
            }

            var result = unitId.HasValue ? stockService.GetStockListForUnit<MedicineStock, Medicine>(unitId.Value, page, pageSize, null, whereCondition) :
                stockService.GetStockList<MedicineStock, Medicine>(page, pageSize, null, whereCondition);

            return Ok(result);
        }

        [HttpGet("MedSupply/{id}")]
        public IActionResult GetMedSupplyStockDetail(int id, int pageSize = 10, int page = 1, int? unitId = null, string where = null)
        {
            Expression<Func<MedicalSupplyStock, bool>> whereCondition = x => x.ItemId == id;
            if (where != null)
            {
                var searcher = new StockItemSearch<MedicalSupplyStock, MedicalSupply>(tz);
                whereCondition = whereCondition.AndAlso(searcher.GetWhereCondition(where));
            }

            var result = unitId.HasValue ? stockService.GetStockListForUnit<MedicalSupplyStock, MedicalSupply>(unitId.Value, page, pageSize, null, whereCondition) :
                stockService.GetStockList<MedicalSupplyStock, MedicalSupply>(page, pageSize, null, whereCondition);

            return Ok(result);
        }

        [HttpGet("Equipment/{id}")]
        public IActionResult GetEquipmentStockDetail(int id, int pageSize = 10, int page = 1, int? unitId = null, string where = null)
        {
            Expression<Func<EquipmentStock, bool>> whereCondition = x => x.ItemId == id;
            if (where != null)
            {
                var searcher = new StockItemSearch<EquipmentStock, Equipment>(tz);
                whereCondition = whereCondition.AndAlso(searcher.GetWhereCondition(where));
            }

            var result = unitId.HasValue ? stockService.GetStockListForUnit<EquipmentStock, Equipment>(unitId.Value, page, pageSize, null, whereCondition) :
                stockService.GetStockList<EquipmentStock, Equipment>(page, pageSize, null, whereCondition);

            return Ok(result);
        }

        [HttpGet("Dialyzer/{id}")]
        public IActionResult GetDialyzerStockDetail(int id, int pageSize = 10, int page = 1, int? unitId = null, string where = null)
        {
            Expression<Func<DialyzerStock, bool>> whereCondition = x => x.ItemId == id;
            if (where != null)
            {
                var searcher = new StockItemSearch<DialyzerStock, Dialyzer>(tz);
                whereCondition = whereCondition.AndAlso(searcher.GetWhereCondition(where));
            }

            var result = unitId.HasValue ? stockService.GetStockListForUnit<DialyzerStock, Dialyzer>(unitId.Value, page, pageSize, null, whereCondition) :
                stockService.GetStockList<DialyzerStock, Dialyzer>(page, pageSize, null, whereCondition);

            return Ok(result);
        }

        // =========== Add ==================

        [HttpPost("Medicine")]
        public IActionResult AddNewMedStock(StockItemViewModel data)
        {
            var add = mapper.Map<MedicineStock>(data);
            stockService.AddStockItem<MedicineStock, Medicine>(add);

            return Ok(mapper.Map<StockItemViewModel>(add));
        }

        [HttpPost("MedSupply")]
        public IActionResult AddNewMedSupplyStock(StockItemViewModel data)
        {
            var add = mapper.Map<MedicalSupplyStock>(data);
            stockService.AddStockItem<MedicalSupplyStock, MedicalSupply>(add);

            return Ok(mapper.Map<StockItemViewModel>(add));
        }

        [HttpPost("Equipment")]
        public IActionResult AddNewEquipmentStock(StockItemViewModel data)
        {
            var add = mapper.Map<EquipmentStock>(data);
            stockService.AddStockItem<EquipmentStock, Equipment>(add);

            return Ok(mapper.Map<StockItemViewModel>(add));
        }

        [HttpPost("Dialyzer")]
        public IActionResult AddNewDialyzerStock(StockItemViewModel data)
        {
            var add = mapper.Map<DialyzerStock>(data);
            stockService.AddStockItem<DialyzerStock, Dialyzer>(add);

            return Ok(mapper.Map<StockItemViewModel>(add));
        }

        // =========== Edit ==================

        [HttpPost("Medicine/{id}")]
        public IActionResult EditMedStock(Guid id, StockItemViewModel data)
        {
            var edit = mapper.Map<MedicineStock>(data);
            data.Id = id;
            var result = stockService.UpdateStockItem<MedicineStock, Medicine>(edit);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("MedSupply/{id}")]
        public IActionResult EditMedSupplyStock(Guid id, StockItemViewModel data)
        {
            var edit = mapper.Map<MedicalSupplyStock>(data);
            data.Id = id;
            var result = stockService.UpdateStockItem<MedicalSupplyStock, MedicalSupply>(edit);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("Equipment/{id}")]
        public IActionResult EditEquipmentStock(Guid id, StockItemViewModel data)
        {
            var edit = mapper.Map<EquipmentStock>(data);
            data.Id = id;
            var result = stockService.UpdateStockItem<EquipmentStock, Equipment>(edit);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("Dialyzer/{id}")]
        public IActionResult EditDialyzerStock(Guid id, StockItemViewModel data)
        {
            var edit = mapper.Map<DialyzerStock>(data);
            data.Id = id;
            var result = stockService.UpdateStockItem<DialyzerStock, Dialyzer>(edit);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        // =========== Add or Update Bulk ==================

        [HttpPost("Medicine/bulk")]
        public IActionResult BulkUpdateMedStock(StockItemBulkViewModel request)
        {
            var data = mapper.Map<IEnumerable<MedicineStock>>(request.Data);
            var removeList = mapper.Map<IEnumerable<MedicineStock>>(request.RemoveList);

            var result = stockService.BulkUpdateStockItem<MedicineStock, Medicine>(data, removeList);
            if (!result)
            {
                return NotFound();
            }

            return Ok(data);
        }

        [HttpPost("MedSupply/bulk")]
        public IActionResult BulkUpdateMedSupplyStock(StockItemBulkViewModel request)
        {
            var data = mapper.Map<IEnumerable<MedicalSupplyStock>>(request.Data);
            var removeList = mapper.Map<IEnumerable<MedicalSupplyStock>>(request.RemoveList);

            var result = stockService.BulkUpdateStockItem<MedicalSupplyStock, MedicalSupply>(data, removeList);
            if (!result)
            {
                return NotFound();
            }

            return Ok(data);
        }

        [HttpPost("Equipment/bulk")]
        public IActionResult BulkUpdateEquipmentStock(StockItemBulkViewModel request)
        {
            var data = mapper.Map<IEnumerable<EquipmentStock>>(request.Data);
            var removeList = mapper.Map<IEnumerable<EquipmentStock>>(request.RemoveList);

            var result = stockService.BulkUpdateStockItem<EquipmentStock, Equipment>(data, removeList);
            if (!result)
            {
                return NotFound();
            }

            return Ok(data);
        }

        [HttpPost("Dialyzer/bulk")]
        public IActionResult BulkUpdateDialyzerStock(StockItemBulkViewModel request)
        {
            var data = mapper.Map<IEnumerable<DialyzerStock>>(request.Data);
            var removeList = mapper.Map<IEnumerable<DialyzerStock>>(request.RemoveList);

            var result = stockService.BulkUpdateStockItem<DialyzerStock, Dialyzer>(data, removeList);
            if (!result)
            {
                return NotFound();
            }

            return Ok(data);
        }

        // =========== Delete ==================

        [HttpDelete("{id}")]
        public IActionResult DeleteStockItem(Guid id)
        {
            var result = stockService.RemoveStockItem(id);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        
        // ========================= Util ===============================

        private Action<IOrderer<StockOverview<T>>> GetOverviewOrder<T>(List<string> order)
            where T : Stockable
        {
            if (order == null || order.Count == 0)
            {
                return null;
            }

            var split = order[0].ToLower().Split('_');
            string key = split[0];
            bool desc = split.Length > 1 && split[1] == "desc";

            switch (key)
            {
                case "id":
                    return x => x.OrderBy(x => x.ItemInfo.Id, desc);
                case "name":
                    return x => x.OrderBy(x => x.ItemInfo.Name, desc);
                case "unit":
                    return x => x.OrderBy(x => x.UnitId, desc);
                case "sum":
                    return x => x.OrderBy(x => x.Sum, desc);
                default:
                    break;
            }

            return null;
        }

        private Action<IOrderer<StockIntermidiate<TItem, TStock>>> GetIntermidiateOrder<TItem, TStock>(List<string> order)
            where TItem : StockItem<TStock>, new() where TStock : Stockable
        {
            if (order == null || order.Count == 0)
            {
                return null;
            }

            var split = order[0].ToLower().Split('_');
            string key = split[0];
            bool desc = split.Length > 1 && split[1] == "desc";

            switch (key)
            {
                case "id":
                    return x => x.OrderBy(x => x.Item.ItemInfo.Id, desc);
                case "name":
                    return x => x.OrderBy(x => x.Item.ItemInfo.Name, desc);
                case "sum":
                    return x => x.OrderBy(x => x.Sum, desc);
                default:
                    break;
            }

            return null;
        }

    }
}
