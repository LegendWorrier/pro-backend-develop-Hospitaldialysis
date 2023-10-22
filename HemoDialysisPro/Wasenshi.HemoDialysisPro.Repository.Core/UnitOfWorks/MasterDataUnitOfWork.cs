using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class MasterDataUnitOfWork : IMasterDataUOW
    {
        private readonly IContextAdapter _context;
        private readonly Dictionary<Type, Func<object>> _cachedExpressions = new Dictionary<Type, Func<object>>();

        public MasterDataUnitOfWork(IContextAdapter context)
        {
            _context = context;
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }

        public IRepository<TData, TKey> GetMasterRepo<TData, TKey>() where TData : EntityBase<TKey>
        {
            return new Repository<TData, TKey>(_context);
        }

        public IRepository<TData, TKey> GetMasterRepo<TData, TKey, TRepo>()
            where TData : EntityBase<TKey>
            where TRepo : IRepository<TData, TKey>
        {
            var repoType = typeof(TRepo);
            if (!_cachedExpressions.ContainsKey(repoType))
            {
                var func = Expression.Lambda<Func<object>>(Expression.New(repoType.GetConstructor(new[] { typeof(IContextAdapter) }), Expression.Constant(_context))).Compile();
                _cachedExpressions.Add(repoType, func);
            }

            return (TRepo)_cachedExpressions[repoType]();
        }
    }
}
