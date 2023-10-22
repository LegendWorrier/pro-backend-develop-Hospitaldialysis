using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base
{
    public interface IRepository<TEntity> : IRepository<TEntity, int> where TEntity : class, IEntityBase<int> { }
    public interface IRepository<TEntity, TKey> : IRepositoryBase<TEntity> where TEntity : class, IEntityBase<TKey>
    {
        /// <summary>
        /// Get the specific instance of data with all the additional related datas, by key.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity Get(TKey id);

        /// <summary>
        /// Delete all the child-collections of the entity. This is to prevent EF error, or to clear related data.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="memberSelectors"></param>
        void ClearCollection(TEntity entity, params Expression<Func<TEntity, IEnumerable<object>>>[] memberSelectors);

        /// <summary>
        /// Sync the child-collections of the entity. Instead of clearing all, this operation will carefully select what to delete, update, or insert for you.
        /// </summary>
        /// <typeparam name="TCollection"></typeparam>
        /// <typeparam name="TColKey"></typeparam>
        /// <param name="entity"></param>
        /// <param name="memberSelectors"></param>
        void SyncCollection<TCollection, TColKey>(TEntity entity, Expression<Func<TEntity, IEnumerable<TCollection>>> memberSelector, Action<TCollection> beforeDel = null)
            where TCollection : class, IEntityBase<TColKey>;
        /// <summary>
        /// Sync the child-collections of the entity. This variant is used mainly for a collection that is multiple keys.
        /// </summary>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="entity"></param>
        /// <param name="memberSelectors"></param>
        /// <param name="comparer"></param>
        /// <param name="onNew"></param>
        void SyncCollection<TCollection>(TEntity entity,
            Expression<Func<TEntity, IEnumerable<TCollection>>> memberSelector,
            IEqualityComparer<TCollection> comparer,
            Action<TCollection> onNew = null,
            Action<TCollection> beforeDel = null)
            where TCollection : class;
    }

    public interface IRepositoryBase<TEntity> where TEntity : class, IEntityBase
    {
        /// <summary>
        /// Get default universal query of this data. Can optionally specify to include or opt-out the additional related data.
        /// </summary>
        /// <param name="include"> Specify whether to include additional related data for this or not</param>
        /// <returns></returns>
        IQueryable<TEntity> GetAll(bool include = true);
        /// <summary>
        /// Find the specific instance of data with criteria. Can specify to include or opt-out additional related data.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> expression, bool include = true);
        void Insert(TEntity entity);
        EntityEntry<TEntity> Update(TEntity entity);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);



        int Complete();
    }
}
