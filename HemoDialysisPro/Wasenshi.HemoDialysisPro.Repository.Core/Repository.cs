using AutoMapper.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    /// <summary>
    /// Default generic repository. Has 'int' as a key type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : Repository<T, int>, IRepository<T> where T : class, IEntityBase<int>
    {
        public Repository(IContextAdapter contextAdapter) : base(contextAdapter)
        {
        }
    }

    public class Repository<T, Tkey> : RepositoryBase<T>, IRepository<T, Tkey> where T : class, IEntityBase<Tkey>
    {
        public Repository(IContextAdapter contextAdapter) : base(contextAdapter)
        {
        }

        public T Get(Tkey id)
        {
            return GetQueryWithIncludes().AsNoTracking().SingleOrDefault(s => s.Id.Equals(id));
        }

        public void ClearCollection(T entity, params Expression<Func<T, IEnumerable<object>>>[] memberSelectors)
        {
            var old = context.Entry(Get(entity.Id));
            // remove range , then detach the parent entity
            foreach (var selector in memberSelectors)
            {
                var collection = old.Collection(selector).CurrentValue;
                context.RemoveRange(collection);
            }

            old.State = EntityState.Detached;
        }

        public void SyncCollection<TCollection, TColKey>(T entity, Expression<Func<T, IEnumerable<TCollection>>> memberSelector, Action<TCollection> beforeDel = null)
            where TCollection : class, IEntityBase<TColKey>
        {
            var old = context.Entry(Get(entity.Id));

            var currentList = old.Collection(memberSelector).CurrentValue;
            var newList = memberSelector.Compile()(entity);

            var comparer = Activator.CreateInstance(typeof(TCollection)) as IEqualityComparer<IEntityBase<TColKey>>;
            var deletes = currentList.Except(newList, comparer).Cast<TCollection>().ToList();
            if (beforeDel != null)
            {
                deletes.ForEach(x => beforeDel(x));
            }
            context.RemoveRange(deletes);
            foreach (var item in newList.Except(currentList, comparer))
            {
                item.Id = default;
            }

            old.State = EntityState.Detached;
        }

        public void SyncCollection<TCollection>(T entity,
            Expression<Func<T, IEnumerable<TCollection>>> memberSelector,
            IEqualityComparer<TCollection> comparer,
            Action<TCollection> onNew = null,
            Action<TCollection> beforeDel = null
            )
            where TCollection : class
        {
            var old = context.Entry(Get(entity.Id));

            var currentList = old.Collection(memberSelector).CurrentValue;
            var newList = memberSelector.Compile()(entity);

            var deletes = currentList.Except(newList, comparer).ToList();
            foreach (var item in deletes)
            {
                var entry = context.Entry(item);
                foreach (var nav in entry.Navigations)
                {
                    var info = nav.Metadata.GetMemberInfo(false, true);
                    info.SetMemberValue(item, null);
                }
            }
            context.RemoveRange(deletes);
            var entityType = old.Collection(memberSelector).Metadata.TargetEntityType;
            bool isOwned = entityType.IsOwned();
            foreach (var item in newList.Except(currentList, comparer))
            {
                onNew?.Invoke(item);
                if (!isOwned)
                {
                    context.Add(item);
                }
            }

            old.State = EntityState.Detached;
        }
    }
}
