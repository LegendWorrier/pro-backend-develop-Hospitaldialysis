using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IMasterDataService : IApplicationService
    {
        TData AddMasterData<TData, TKey>(TData data) where TData : EntityBase<TKey>, new();
        TData AddMasterData<TData>(TData data) where TData : EntityBase<int>, new();

        IEnumerable<TData> GetMasterDataList<TData, TKey>() where TData : EntityBase<TKey>, new();
        IEnumerable<TData> GetMasterDataList<TData>() where TData : EntityBase<int>, new();

        IEnumerable<TData> GetMasterDataList<TData, TKey, TRepo>() where TData : EntityBase<TKey>, new() where TRepo : IRepository<TData, TKey>;

        bool UpdateMasterData<TData, TKey>(TData data) where TData : EntityBase<TKey>, new();
        bool UpdateMasterData<TData>(TData data) where TData : EntityBase<int>, new();

        bool RemoveMasterData<TData, TKey>(TKey id) where TData : EntityBase<TKey>, new();
        bool RemoveMasterData<TData>(int id) where TData : EntityBase<int>, new();

        // -------- Special Cases ----------
        bool UpdateLabExamItem(LabExamItem item);
        LabExamItem GetLabExamItem(int id);
        IEnumerable<LabExamItem> GetLabExamItemList();

        IEnumerable<Stockable> GetStockableList(Expression<Func<Stockable, bool>> where);

        IEnumerable<PatientHistoryItem> GetPatientHistoryList();
        bool SwapOrderPatientHistoryItems(int firstId, int secondId);

        /// <summary>
        /// Check maximum limit of units. If limit is reached, return true. Otherwise, return false.
        /// </summary>
        /// <returns></returns>
        bool CheckMaxUnits();

        bool IsUnitHead(Guid userId, int unitId);
        bool IsUnitHead(Guid userId);
    }
}
