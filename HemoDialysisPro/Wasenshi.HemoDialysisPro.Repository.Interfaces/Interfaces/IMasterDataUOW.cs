using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IMasterDataUOW : IUnitOfWork
    {
        IRepository<TData, TKey> GetMasterRepo<TData, TKey>() where TData : EntityBase<TKey>;
        IRepository<TData, TKey> GetMasterRepo<TData, TKey, TRepo>() where TData : EntityBase<TKey> where TRepo : IRepository<TData, TKey>;
    }
}