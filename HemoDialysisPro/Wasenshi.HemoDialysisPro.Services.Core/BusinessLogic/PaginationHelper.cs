using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public static class PaginationHelper
    {
        /// <summary>
        /// How many row in the DB before we switch to advance pagination
        /// </summary>
        public static int MagicNumber = 500;

        public static Page<TEntity> GetPagination<TEntity>(this IQueryable<TEntity> query,
            int pageSize, int pageIndex, Action<IOrderer<TEntity>> orderBy, Expression<Func<TEntity, bool>> filter = null, IQueryable<TEntity> countQuery = null) where TEntity : class
        {
            if (pageSize * pageIndex >= MagicNumber)
            {
                return AdvancePagination(query, pageSize, pageIndex, orderBy, filter, countQuery);
            }
            return BasicPagination(query, pageSize, pageIndex, orderBy, filter, countQuery);
        }

        private static Page<TEntity> BasicPagination<TEntity>(IQueryable<TEntity> query, int pageSize, int pageIndex, Action<IOrderer<TEntity>> orderBy,
            Expression<Func<TEntity, bool>> filter, IQueryable<TEntity> queryCount) where TEntity : class
        {
            if (filter != null)
            {
                query = query.Where(filter);
            }

            List<TEntity> data;
            if (orderBy != null)
            {
                var orderer = new Orderer<TEntity>(query);
                orderBy.Invoke(orderer);
                data = orderer.GetOrderedQueryable().Skip(pageSize * pageIndex).Take(pageSize).ToList();
            }
            else
            {
                data = query.Skip(pageSize * pageIndex).Take(pageSize).ToList();
            }

            var count = queryCount?.Count() ?? query.Count();

            return new Page<TEntity>
            {
                Data = data,
                Total = count
            };
        }

        private static Page<TEntity> AdvancePagination<TEntity>(this IQueryable<TEntity> query,
            int pageSize, int pageIndex, Action<IOrderer<TEntity>> orderBy,
            Expression<Func<TEntity, bool>> filter = null, IQueryable<TEntity> countQuery = null) where TEntity : class
        {
            if (filter != null)
            {
                query = query.Where(filter);
            }

            var count = countQuery?.Count() ?? query.Count();
            if (count == 0)
            {
                return new Page<TEntity>
                {
                    Data = new List<TEntity>(),
                    Total = 0
                };
            }

            var orderer = new Orderer<TEntity>(query);
            orderBy?.Invoke(orderer);

            List<TEntity> data;

            int offset = pageIndex * pageSize;
            int limit = pageSize;

            var isReverse = offset > count / 2;
            // If we're skipping more than half of all records, then swap the ordering
            if (isReverse)
            {
                offset = count - (pageIndex + 1) * pageSize;
                limit = offset < 0 ? pageSize + offset : pageSize;
                offset = Math.Max(0, offset);

                orderer.OrderList.ForEach(x => x.IsDesc = !x.IsDesc);
            }

            if (orderBy != null)
            {
                data = orderer.GetOrderedQueryable().Skip(offset).Take(limit).ToList();
            }
            else
            {
                data = query.Skip(offset).Take(limit).ToList();
            }

            if (isReverse)
            {
                data.Reverse();
            }

            return new Page<TEntity>
            {
                Data = data,
                Total = count
            };
        }
    }
}
