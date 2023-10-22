using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Wasenshi.HemoDialysisPro.Models
{
    public interface IOrderer<T>
    {
        IOrderer<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool desc = false);

        IOrderer<T> Default<TKey>(Expression<Func<T, TKey>> keySelector, bool desc = false);
    }

    public class Orderer<T> : IOrderer<T>
    {
        private IQueryable<T> source;
        private OrderNode defaultOrdering;
        public Orderer<T> SetNewSource(IQueryable<T> source)
        {
            this.source = source;
            return this;
        }

        private readonly List<OrderNode> orderingMaps =
            new List<OrderNode>();

        private readonly List<Expression<Func<T, bool>>> whereList = new List<Expression<Func<T, bool>>>();

        public bool HasOrder => OrderList.Any();
        public bool IsDesc => !HasOrder || OrderList[0].IsDesc;
        public Type MainKeyType => OrderList[0].KeyType;
        public LambdaExpression MainKeySelector => OrderList[0].KeySelector;
        public List<OrderNode> OrderList => orderingMaps;

        public Orderer(IQueryable<T> source)
        {
            this.source = source;
        }

        public Orderer(Action<IOrderer<T>> orderCmd)
        {
            orderCmd(this);
        }

        public IOrderer<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool desc = false)
        {
            orderingMaps.Add(new OrderNode(keySelector, desc));
            return this;
        }

        public IOrderer<T> Default<TKey>(Expression<Func<T, TKey>> keySelector, bool desc = false)
        {
            defaultOrdering = new OrderNode(keySelector, desc);
            return this;
        }

        public Orderer<T> AddWhereCause(Expression<Func<T, bool>> whereExpression)
        {
            whereList.Add(whereExpression);
            return this;
        }

        public IOrderedQueryable<T> GetOrderedQueryable()
        {
            if (source == null)
            {
                throw new InvalidOperationException("The source query is empty. Please add new source qeury first.");
            }

            var query = whereList.Aggregate(source, (current, @where) => current.Where(@where));

            if (!HasOrder)
            {
                if (defaultOrdering == null)
                {
                    throw new InvalidOperationException("Cannot order the source query. There is no default nor any ordering. Please add the default, or any ordering first.");
                }

                return Order(query, defaultOrdering);
            }

            var orderedQuery = Order(query, OrderList[0]);
            return OrderList.Skip(1).Aggregate(orderedQuery, Order);
        }

        private IOrderedQueryable<T> Order(IQueryable<T> query, OrderNode ordering)
        {
            return ordering.IsDesc ? query.OrderByDescending(ordering.KeySelector) : query.OrderBy(ordering.KeySelector);
        }

        private IOrderedQueryable<T> Order(IOrderedQueryable<T> query, OrderNode ordering)
        {
            return ordering.IsDesc ? query.ThenByDescending(ordering.KeySelector) : query.ThenBy(ordering.KeySelector);
        }

        public class OrderNode
        {
            public LambdaExpression KeySelector { get; set; }
            public bool IsDesc { get; set; }
            public Type KeyType => KeySelector.ReturnType;

            public OrderNode(LambdaExpression keySelector, bool isDesc = false)
            {
                KeySelector = keySelector;
                IsDesc = isDesc;
            }
        }
    }

    public static class QueryHelper
    {
        // ------ ↓ Just for reference, Not used anymore ↓ --------
        public static readonly MethodInfo ConcatStringMethod = typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) });

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    typeof(Queryable),
                    "OrderBy",
                    new[] { typeof(TSource), keySelector.ReturnType },
                    source.Expression, Expression.Quote(keySelector)
                ));
        }

        public static IOrderedQueryable<TSource> OrderByDescending<TSource>(this IQueryable<TSource> source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    typeof(Queryable),
                    "OrderByDescending",
                    new[] { typeof(TSource), keySelector.ReturnType },
                    source.Expression, Expression.Quote(keySelector)
                ));
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    typeof(Queryable),
                    "ThenBy",
                    new[] { typeof(TSource), keySelector.ReturnType },
                    source.Expression, Expression.Quote(keySelector)
                ));
        }

        public static IOrderedQueryable<TSource> ThenByDescending<TSource>(this IOrderedQueryable<TSource> source, LambdaExpression keySelector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    typeof(Queryable),
                    "ThenByDescending",
                    new[] { typeof(TSource), keySelector.ReturnType },
                    source.Expression, Expression.Quote(keySelector)
                ));
        }

        // ------ ↓ Just for reference, Not used anymore ↓ --------
        public static void RegisterDbExtension(this ModelBuilder builder)
        {
            builder.HasDbFunction(typeof(DbContextExtended).GetMethod(nameof(DbContextExtended.ToString),
                new[] { typeof(DateTime) })).HasTranslation(x => x.First());
            builder.HasDbFunction(typeof(DbContextExtended).GetMethod(nameof(DbContextExtended.ToString),
                new[] { typeof(DateTime) })).HasTranslation(x => x.First());
        }
    }
    // ------ ↓ Just for reference, Not used anymore ↓ --------
    public class DbContextExtended : DbContext
    {
        public static string ToString(DateTime obj)
        {
            throw new NotImplementedException();
        }

        public static string ToString(int obj)
        {
            throw new NotImplementedException();
        }
    }
}
