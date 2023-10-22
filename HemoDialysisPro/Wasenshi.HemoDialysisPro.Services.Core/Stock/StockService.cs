using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Utils;
using Zomp.EFCore.WindowFunctions;

namespace Wasenshi.HemoDialysisPro.Services.Core.Stock
{
    public class StockService : IStockService
    {
        private readonly IContextAdapter context;
        private readonly IMapper mapper;

        public StockService(IContextAdapter context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public StockItem GetStockItemById(Guid id)
        {
            StockItemBase stockItem = context.Context.Set<MedicineStock>().Include(x => x.ItemInfo).FirstOrDefault(x => x.Id == id);
            if (stockItem != null)
            {
                var result = mapper.Map<MedicineStock, StockItem>(stockItem as MedicineStock);
                result.StockableType = typeof(Medicine).Name;
                return result;
            }
            stockItem = context.Context.Set<MedicalSupplyStock>().Include(x => x.ItemInfo).FirstOrDefault(x => x.Id == id);
            if (stockItem != null)
            {
                var result = mapper.Map<MedicalSupplyStock, StockItem>(stockItem as MedicalSupplyStock);
                result.StockableType = typeof(MedicalSupply).Name;
                return result;
            }
            stockItem = context.Context.Set<DialyzerStock>().Include(x => x.ItemInfo).FirstOrDefault(x => x.Id == id);
            if (stockItem != null)
            {
                var result = mapper.Map<DialyzerStock, StockItem>(stockItem as DialyzerStock);
                result.StockableType = typeof(Dialyzer).Name;
                return result;
            }
            stockItem = context.Context.Set<EquipmentStock>().Include(x => x.ItemInfo).FirstOrDefault(x => x.Id == id);
            if (stockItem != null)
            {
                var result = mapper.Map<EquipmentStock, StockItem>(stockItem as EquipmentStock);
                result.StockableType = typeof(Equipment).Name;
                return result;
            }

            return null;
        }

        public TItem AddStockItem<TItem, TStock>(TItem data)
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
        {
            context.Context.Add(data);
            context.Context.SaveChanges();
            return data;
        }

        public bool UpdateStockItem<TItem, TStock>(TItem data)
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
        {
            context.Context.Update(data);
            return context.Context.SaveChanges() > 0;
        }

        public bool RemoveStockItem(Guid id)
        {
            context.Context.Remove(new StockItemBase { Id = id });
            return context.Context.SaveChanges() > 0;
        }

        public bool BulkUpdateStockItem<TItem, TStock>(IEnumerable<TItem> data, IEnumerable<TItem> removeList)
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
        {
            context.Context.UpdateRange(data);
            if (removeList?.Any() ?? false)
            {
                context.Context.RemoveRange(removeList);
            }

            return context.Context.SaveChanges() > 0;
        }

        public bool AddStockLot(IEnumerable<StockItemBase> lot)
        {
            foreach (var item in lot)
            {
                if (item is MedicineStock med)
                {
                    context.Context.Add(med);
                }
                else if (item is MedicalSupplyStock supply)
                {
                    context.Context.Add(supply);
                }
                else if (item is DialyzerStock dialyzer)
                {
                    context.Context.Add(dialyzer);
                }
                else if (item is EquipmentStock equipment)
                {
                    context.Context.Add(equipment);
                }
                else
                {
                    throw new AppException("UNKNOWN", "Unknown stok type.");
                }
            }

            return context.Context.SaveChanges() > 0;
        }

        public Page<TItem> GetStockList<TItem, TStock>(int page = 1, int limit = 25, Action<IOrderer<StockIntermidiate<TItem, TStock>>> orderBy = null, Expression<Func<TItem, bool>> whereCondition = null)
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
        {
            IQueryable<TItem> initQuery = context.Context.Set<TItem>();
            if (whereCondition != null)
            {
                initQuery = initQuery.Where(whereCondition);
            }
            var query = initQuery
                .Select(x => new
                {
                    Item = x,
                    Qt = x.IsCredit ? -x.Quantity : x.Quantity
                })
                .Select(x => new StockIntermidiate<TItem, TStock>
                {
                    Item = x.Item,
                    Sum = (int)EF.Functions.Sum((decimal)x.Qt, EF.Functions.Over().OrderByDescending(x.Item.EntryDate).Rows().FromCurrentRow().ToUnbounded())
                });

            void Ordering(IOrderer<StockIntermidiate<TItem, TStock>> order)
            {
                order.Default(x => x.Item.EntryDate, true); // Default order by entry date
                orderBy?.Invoke(order); // Followed by custom ordering from client or controller
            }

            var pageResult = query.GetPagination(limit, page - 1, Ordering);

            return new Page<TItem>
            {
                Data = pageResult.Data.Select(x =>
                {
                    x.Item.Sum = x.Sum;
                    return x.Item;
                }),
                Total = pageResult.Total
            };
        }

        public Page<TItem> GetStockListForUnit<TItem, TStock>(int unitId, int page = 1, int limit = 25, Action<IOrderer<StockIntermidiate<TItem, TStock>>> orderBy = null,
            Expression<Func<TItem, bool>> whereCondition = null)
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
        {
            Expression<Func<TItem, bool>> unitFilter = x => x.UnitId == unitId;
            return GetStockList(page, limit, orderBy, unitFilter.AndAlso(whereCondition));
        }

        public Page<StockOverview<TStock>> GetStockOverview<TItem, TStock>(int page = 1, int limit = 25,
            Action<IOrderer<StockOverview<TStock>>> orderBy = null,
            Expression<Func<TItem, bool>> whereCondition = null)
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
        {
            IQueryable<TItem> initQuery = context.Context.Set<TItem>()
                .Include(x => x.ItemInfo);

            if (whereCondition != null)
            {
                initQuery = initQuery.Where(whereCondition);
            }

            var groupingQuery = initQuery.Select(x => new
                   {
                       Item = x,
                       Qt = (x.IsCredit ? -x.Quantity : x.Quantity)
                   })
                .GroupBy(x => new { x.Item.ItemId, x.Item.ItemInfo.Name });

            Action < IOrderer < StockOverview < TStock >>> order = null;
            if (orderBy != null)
            {
                var orderer = new Orderer<StockOverview<TStock>>(orderBy);
                var ordering = orderer.OrderList[0]; // allow only simple order for this particular action
                var key = (ordering.KeySelector.Body as MemberExpression).Member.Name;
                var desc = ordering.IsDesc;

                switch (key)
                {
                    case nameof(StockOverview<TStock>.ItemInfo.Id):
                        groupingQuery = desc ? groupingQuery.OrderByDescending(x => x.Key.ItemId) : groupingQuery.OrderBy(x => x.Key.ItemId);
                        break;
                    case nameof(StockOverview<TStock>.ItemInfo.Name):
                        groupingQuery = desc ? groupingQuery.OrderByDescending(x => x.Key.Name) : groupingQuery.OrderBy(x => x.Key.Name);
                        break;
                    case nameof(StockOverview<TStock>.Sum):
                        order = x => x.OrderBy(x => x.Sum, desc);
                        break;
                    case nameof(StockOverview<TStock>.UnitId):
                        order = x => x.OrderBy(x => x.UnitId, desc);
                        break;
                }
            }

            var query = groupingQuery.Select(x => new StockOverview<TStock>
            {
                Sum = x.Sum(a => a.Qt),
                ItemInfo = x.FirstOrDefault().Item.ItemInfo,
                UnitId = x.FirstOrDefault().Item.UnitId
            });

            var countQuery = initQuery
                .GroupBy(x => x.ItemId)
                .Select(x => new StockOverview<TStock>
                {
                });

            var pageResult = query.GetPagination(limit, page - 1, order, null, countQuery);

            return pageResult;
        }

        public Page<StockOverview<TStock>> GetStockOverviewForUnit<TItem, TStock>(int unitId, int page = 1, int limit = 25,
            Action<IOrderer<StockIntermidiate<TItem, TStock>>> orderBy = null,
            Expression<Func<TItem, bool>> whereCondition = null)
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
        {
            IQueryable<TItem> initQuery = context.Context.Set<TItem>().Include(x => x.ItemInfo);
            if (whereCondition != null)
            {
                initQuery = initQuery.Where(whereCondition);
            }
            var groupingQuery = initQuery
                    .Where(x => x.UnitId == unitId)
                   .Select(x => new
                   {
                       Item = x,
                       Qt = x.IsCredit ? -x.Quantity : x.Quantity
                   })
                .GroupBy(x => new { x.Item.ItemId, x.Item.ItemInfo.Name });

            Action<IOrderer<StockIntermidiate<TItem, TStock>>> order = null;
            if (orderBy != null)
            {
                var orderer = new Orderer<StockIntermidiate<TItem, TStock>>(orderBy);
                var ordering = orderer.OrderList[0]; // allow only simple order for this particular action
                var key = (ordering.KeySelector.Body as MemberExpression).Member.Name;
                var desc = ordering.IsDesc;

                switch (key)
                {
                    case nameof(StockIntermidiate<TItem, TStock>.Item.ItemInfo.Id):
                        groupingQuery = desc ? groupingQuery.OrderByDescending(x => x.Key.ItemId) : groupingQuery.OrderBy(x => x.Key.ItemId);
                        break;
                    case nameof(StockIntermidiate<TItem, TStock>.Item.ItemInfo.Name):
                        groupingQuery = desc ? groupingQuery.OrderByDescending(x => x.Key.Name) : groupingQuery.OrderBy(x => x.Key.Name);
                        break;
                    case nameof(StockIntermidiate<TItem, TStock>.Sum):
                        order = x => x.OrderBy(x => x.Sum, desc);
                        break;
                }
            }

            var query = groupingQuery.Select(x => new StockIntermidiate<TItem, TStock>
            {
                Sum = x.Sum(a => a.Qt),
                Item = x.First().Item
            });

            var countQuery = initQuery
                .Where(x => x.UnitId == unitId)
                .GroupBy(x => x.ItemId)
                .Select(x => new StockIntermidiate<TItem, TStock>
                {
                });

            var pageResult = query.GetPagination(limit, page - 1, order, null, countQuery);

            return new Page<StockOverview<TStock>>
            {
                Data = pageResult.Data.Select(x =>
                {
                    return new StockOverview<TStock>
                    {
                        ItemInfo = x.Item.ItemInfo,
                        UnitId = x.Item.UnitId,
                        Sum = x.Sum
                    };
                }),
                Total = pageResult.Total
            };
        }

    }

    /// <summary>
    /// Only used for query and mapping. The sum value will be put into each stock item itself when returning to FE.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TStock"></typeparam>
    public class StockIntermidiate<TItem, TStock>
            where TItem : StockItem<TStock>, new()
            where TStock : Stockable
    {
        public TItem Item { get; set; }
        public int Sum { get; set; }
    }

    public class StockOverview<TStock>
        where TStock : Stockable
    {
        public TStock ItemInfo { get; set; }
        public int UnitId { get; set; }
        public int Sum { get; set; }
    }
}
