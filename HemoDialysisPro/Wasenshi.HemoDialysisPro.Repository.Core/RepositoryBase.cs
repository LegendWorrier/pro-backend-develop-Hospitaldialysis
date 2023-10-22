using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class, IEntityBase
    {
        protected readonly IApplicationDbContext context;
        protected DbSet<T> _entities;

        public RepositoryBase(IContextAdapter contextAdapter)
        {
            this.context = contextAdapter.Context;
            _entities = context.Set<T>();
        }

        protected virtual IQueryable<T> GetQueryWithIncludes()
        {
            return _entities;
        }

        public IQueryable<T> GetAll(bool include = true)
        {
            return include ? GetQueryWithIncludes().AsNoTracking() : _entities.AsNoTracking();
        }

        public void Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _entities.Add(entity);
        }

        public EntityEntry<T> Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return _entities.Update(entity);
        }

        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            _entities.Remove(entity);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> expression, bool include = true)
        {
            return GetAll(include).Where(expression);
        }

        public int Complete()
        {
            return context.SaveChanges();
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(_entities));
            }
            _entities.RemoveRange(entities);
        }
    }
}
