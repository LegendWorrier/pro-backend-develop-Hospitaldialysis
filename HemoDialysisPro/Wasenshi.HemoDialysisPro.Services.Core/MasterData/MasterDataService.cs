using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class MasterDataService : IMasterDataService
    {
        private readonly IMasterDataUOW masterData;

        private readonly Dictionary<Type, object> scopeCache = new Dictionary<Type, object>();

        public MasterDataService(IMasterDataUOW masterData)
        {
            this.masterData = masterData;
        }

        public TData AddMasterData<TData, TKey>(TData data) where TData : EntityBase<TKey>, new()
        {
            var repo = masterData.GetMasterRepo<TData, TKey>();
            repo.Insert(data);
            repo.Complete();
            return data;
        }

        public IEnumerable<TData> GetMasterDataList<TData, TKey>() where TData : EntityBase<TKey>, new()
        {
            var repo = masterData.GetMasterRepo<TData, TKey>();
            return repo.GetAll().ToList();
        }

        public bool UpdateMasterData<TData, TKey>(TData data) where TData : EntityBase<TKey>, new()
        {
            var repo = masterData.GetMasterRepo<TData, TKey>();
            repo.Update(data);
            return repo.Complete() > 0;
        }

        public bool RemoveMasterData<TData, TKey>(TKey id) where TData : EntityBase<TKey>, new()
        {
            var repo = masterData.GetMasterRepo<TData, TKey>();
            repo.Delete(new TData { Id = id });
            return repo.Complete() > 0;
        }

        public IEnumerable<TData> GetMasterDataList<TData, TKey, TRepo>() where TData : EntityBase<TKey>, new() where TRepo : IRepository<TData, TKey>
        {
            var repo = masterData.GetMasterRepo<TData, TKey, TRepo>();
            return repo.GetAll().ToList();
        }

        // ------------------- Short-cut Method ---------------------------
        public TData AddMasterData<TData>(TData data) where TData : EntityBase<int>, new() { return AddMasterData<TData, int>(data); }
        public IEnumerable<TData> GetMasterDataList<TData>() where TData : EntityBase<int>, new() { return GetMasterDataList<TData, int>(); }
        public bool UpdateMasterData<TData>(TData data) where TData : EntityBase<int>, new() { return UpdateMasterData<TData, int>(data); }
        public bool RemoveMasterData<TData>(int id) where TData : EntityBase<int>, new() { return RemoveMasterData<TData, int>(id); }

        // ---------------- Special Cases -----------------------
        public bool UpdateLabExamItem(LabExamItem item)
        {
            var repo = masterData.GetMasterRepo<LabExamItem, int>();
            var entry = repo.Update(item);
            entry.Property(x => x.IsSystemBound).IsModified = false;
            entry.Property(x => x.Bound).IsModified = false;
            entry.Property(x => x.IsCalculated).IsModified = false;

            return repo.Complete() > 0;
        }

        public LabExamItem GetLabExamItem(int id)
        {
            var repo = masterData.GetMasterRepo<LabExamItem, int>();
            return repo.Get(id);
        }

        public IEnumerable<LabExamItem> GetLabExamItemList()
        {
            var repo = masterData.GetMasterRepo<LabExamItem, int>();
            return repo.GetAll().Where(x => !x.IsCalculated).ToList();
        }

        public IEnumerable<Stockable> GetStockableList(Expression<Func<Stockable, bool>> where)
        {
            where ??= (_) => true;
            var med = masterData.GetMasterRepo<Medicine, int>().GetAll().Where(where).ToList();
            var supply = masterData.GetMasterRepo<MedicalSupply, int>().GetAll().Where(where).ToList();
            var dialyzer = masterData.GetMasterRepo<Dialyzer, int>().GetAll().Where(where).ToList();
            var equipment = masterData.GetMasterRepo<Equipment, int>().GetAll().Where(where).ToList();

            var result = new List<Stockable>();
            result.AddRange(med);
            result.AddRange(dialyzer);
            result.AddRange(supply);
            result.AddRange(equipment);

            return result;
        }

        public IEnumerable<PatientHistoryItem> GetPatientHistoryList()
        {
            var repo = masterData.GetMasterRepo<PatientHistoryItem, int>();
            return repo.GetAll().Include(x => x.Choices.OrderBy(c => c.Id)).OrderBy(x => x.Order).ToList();
        }

        public bool SwapOrderPatientHistoryItems(int firstId, int secondId)
        {
            var repo = masterData.GetMasterRepo<PatientHistoryItem, int>();
            var first = repo.Get(firstId);
            var second = repo.Get(secondId);
            int tmp = first.Order;
            first.Order = second.Order;
            second.Order = tmp;

            repo.Update(first);
            repo.Update(second);

            return repo.Complete() > 0;
        }

        public bool IsUnitHead(Guid userId, int unitId)
        {
            if (!scopeCache.ContainsKey(typeof(Unit)))
            {
                scopeCache.Add(typeof(Unit), masterData.GetMasterRepo<Unit, int>().GetAll(false).ToList());
            }
            var unit = (scopeCache[typeof(Unit)] as IEnumerable<Unit>).FirstOrDefault(x => x.Id == unitId);
            return userId == unit?.HeadNurse;
        }

        public bool IsUnitHead(Guid userId)
        {
            if (!scopeCache.ContainsKey(typeof(Unit)))
            {
                scopeCache.Add(typeof(Unit), masterData.GetMasterRepo<Unit, int>().GetAll(false).ToList());
            }
            var units = (scopeCache[typeof(Unit)] as IEnumerable<Unit>);
            return units.Any(x => userId == x.HeadNurse);
        }

        public bool CheckMaxUnits()
        {
            int totalUnitsCount = masterData.GetMasterRepo<Unit, int>().GetAll(false).Count();
            return totalUnitsCount >= LicenseManager.MaxUnits;
        }
    }
}
