using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Services.Core.Stock;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IStockService : IApplicationService
    {
        Page<TItem> GetStockList<TItem, TStock>(int page = 1, int limit = 25,
            Action<IOrderer<StockIntermidiate<TItem, TStock>>> orderBy = null,
            Expression<Func<TItem, bool>> whereCondition = null)
             where TItem : StockItem<TStock>, new() where TStock : Stockable;
        Page<TItem> GetStockListForUnit<TItem, TStock>(int unitId, int page = 1, int limit = 25,
            Action<IOrderer<StockIntermidiate<TItem, TStock>>> orderBy = null,
            Expression<Func<TItem, bool>> whereCondition = null)
             where TItem : StockItem<TStock>, new() where TStock : Stockable;

        Page<StockOverview<TStock>> GetStockOverview<TItem, TStock>(int page = 1, int limit = 25,
            Action<IOrderer<StockOverview<TStock>>> orderBy = null,
            Expression<Func<TItem, bool>> whereCondition = null)
            where TItem : StockItem<TStock>, new() where TStock : Stockable;
        Page<StockOverview<TStock>> GetStockOverviewForUnit<TItem, TStock>(int unitId, int page = 1, int limit = 25,
            Action<IOrderer<StockIntermidiate<TItem, TStock>>> orderBy = null,
            Expression<Func<TItem, bool>> whereCondition = null)
             where TItem : StockItem<TStock>, new() where TStock : Stockable;

        TItem AddStockItem<TItem, TStock>(TItem data) where TItem : StockItem<TStock>, new() where TStock : Stockable;
        bool UpdateStockItem<TItem, TStock>(TItem data) where TItem : StockItem<TStock>, new() where TStock : Stockable;
        bool RemoveStockItem(Guid id);

        bool BulkUpdateStockItem<TItem, TStock>(IEnumerable<TItem> data, IEnumerable<TItem> removeList) where TItem : StockItem<TStock>, new() where TStock : Stockable;
        bool AddStockLot(IEnumerable<StockItemBase> lot);

        StockItem GetStockItemById(Guid id);
    }
}
