using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IRepository<AssessmentGroup, int> groupRepo;
        private readonly IAssessmentRepository assessmentRepo;
        private readonly IDialysisRecordAssessmentItemRepository drItemRepository;
        private readonly IAssessmentItemRepository itemRepository;
        private readonly IAssessmentUnitOfWork uow;

        public AssessmentService(
            IRepository<AssessmentGroup, int> groupRepo,
            IAssessmentRepository assessmentRepo,
            IDialysisRecordAssessmentItemRepository drItemRepository,
            IAssessmentItemRepository itemRepository,
            IAssessmentUnitOfWork uow)
        {
            this.groupRepo = groupRepo;
            this.assessmentRepo = assessmentRepo;
            this.drItemRepository = drItemRepository;
            this.itemRepository = itemRepository;
            this.uow = uow;
        }

        public IEnumerable<Assessment> GetAllAssessments()
        {
            var result = assessmentRepo
                .GetAll()
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Order);
            return result.ToList();
        }

        public IEnumerable<AssessmentGroup> GetAllAssessmentGroups()
        {
            var result = groupRepo
                .GetAll()
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Order)
                .ToList();
            return result;
        }

        public Assessment GetAssessment(long id)
        {
            return assessmentRepo.Get(id);
        }

        public void RemoveAssessment(long id)
        {
            assessmentRepo.Delete(new Assessment { Id = id });

            assessmentRepo.Complete();
        }

        public void AddAssessment(Assessment assessment)
        {
            assessmentRepo.Insert(assessment);

            assessmentRepo.Complete();
        }

        public bool UpdateAssessment(Assessment assessment)
        {
            if (!assessment.Multi && assessment.OptionsList.Count(x => x.IsDefault) > 1)
            {
                throw new AppException("SINGLE", "Cannot have multiple default for single type.");
            }
            assessmentRepo.SyncCollection<AssessmentOption, long>(assessment, a => a.OptionsList);
            assessmentRepo.Update(assessment);

            return assessmentRepo.Complete() > 0;
        }

        public void ReorderAssessments(long first, long second)
        {
            var a = assessmentRepo.Get(first);
            var b = assessmentRepo.Get(second);
            if (a == null || b == null)
            {
                return;
            }

            if (a.Type != b.Type)
            {
                throw new InvalidOperationException("cannot reorder different types of assessments.");
            }

            (b.Order, a.Order) = (a.Order, b.Order);
            //auto fix data (just in case)
            if (a.Order == b.Order)
            {
                b.Order++;
            }

            assessmentRepo.Update(a);
            assessmentRepo.Update(b);

            assessmentRepo.Complete();
        }

        public IEnumerable<AssessmentItem> GetHemosheetAssessmentItems(Guid hemosheetId)
        {
            var result = itemRepository.GetAll()
                .Where(x => x.HemosheetId == hemosheetId);
            return result;
        }

        public AssessmentItem GetItem(Guid id)
        {
            var result = itemRepository.Get(id);
            return result;
        }

        public int AddOrUpdateItems(Guid hemosheetId, IEnumerable<AssessmentItem> items)
        {
            foreach (var item in items)
            {
                item.HemosheetId = hemosheetId;
            }
            itemRepository.BulkInsertOrUpdate(items);

            return itemRepository.Complete();
        }

        public int AddOrUpdateItemsForDialysisRecord(Guid dialysisRecordId, IEnumerable<DialysisRecordAssessmentItem> items)
        {
            foreach (var item in items)
            {
                item.DialysisRecordId = dialysisRecordId;
            }
            drItemRepository.BulkInsertOrUpdate(items);

            return drItemRepository.Complete();
        }

        public bool RemoveItem(Guid itemId)
        {
            itemRepository.Delete(new AssessmentItem { Id = itemId });

            return itemRepository.Complete() > 0;
        }

        public AssessmentGroup GetGroup(int id)
        {
            return groupRepo.Get(id);
        }

        public void AddGroup(AssessmentGroup group)
        {
            groupRepo.Insert(group);

            groupRepo.Complete();
        }

        public bool UpdateGroup(AssessmentGroup group)
        {
            groupRepo.Update(group);

            return groupRepo.Complete() > 0;
        }

        public void RemoveGroup(int id)
        {
            groupRepo.Delete(new AssessmentGroup { Id = id });

            groupRepo.Complete();
        }

        public void ReorderGroups(int first, int second)
        {
            var a = groupRepo.Get(first);
            var b = groupRepo.Get(second);
            if (a == null || b == null)
            {
                return;
            }

            int tmp = a.Order;
            a.Order = b.Order;
            b.Order = tmp;
            //auto fix data (just in case)
            if (a.Order == b.Order)
            {
                b.Order++;
            }

            groupRepo.Update(a);
            groupRepo.Update(b);

            groupRepo.Complete();
        }

        // ==================== Root Admin ========================

        /// <summary>
        /// This operation will override and erase/update all existing assessments/groups to be up-to-date with the input lists.
        /// This is intented for root admin use only.
        /// </summary>
        public void ImportAllAssessmentAndGroup(IEnumerable<Assessment> assessments, IEnumerable<AssessmentGroup> groups)
        {
            var allGroups = uow.Groups.GetAll(false).ToList();
            var grpDeletes = allGroups.Except(groups, new AssessmentGroup()).Cast<AssessmentGroup>().ToList();
            var grpUpdates = groups.Intersect(allGroups, new AssessmentGroup()).Cast<AssessmentGroup>().ToList();
            var grpInsert = groups.Except(allGroups, new AssessmentGroup()).Cast<AssessmentGroup>().ToList();
            uow.Groups.DeleteRange(grpDeletes);
            foreach (var item in grpUpdates)
            {
                uow.Groups.Update(item);
            }
            foreach (var item in grpInsert)
            {
                uow.Groups.Insert(item);
            }

            var allAssessments = uow.Assessments.GetAll().ToList();
            var deletes = allAssessments.Except(assessments, new Assessment()).Cast<Assessment>().ToList();
            var updates = assessments.Intersect(allAssessments, new Assessment()).Cast<Assessment>().ToList();
            var insert = assessments.Except(allAssessments, new Assessment()).Cast<Assessment>().ToList();
            uow.Assessments.DeleteRange(deletes);
            foreach (var update in updates)
            {
                uow.Assessments.SyncCollection<AssessmentOption, long>(update, a => a.OptionsList);
                uow.Assessments.Update(update);
            }
            foreach (var item in insert)
            {
                item.Id = 0;
                if (item.GroupId.HasValue)
                {
                    var grp = grpInsert.Find(g => g.Id == item.GroupId.Value);
                    if (grp != null)
                    {
                        item.Group = grp;
                        item.GroupId = null;
                    }
                }
                foreach (var option in item.OptionsList)
                {
                    option.Id = 0;
                    option.AssessmentId = 0;
                }
                uow.Assessments.Insert(item);
            }

            uow.Complete();
        }

        /// <summary>
        /// This operation is a short-cut for getting all current assessments/groups.
        /// This is intented for root admin use only.
        /// </summary>
        /// <returns></returns>
        public (IEnumerable<Assessment> assessments, IEnumerable<AssessmentGroup> groups) ExportAllAssessmentAndGroup()
        {
            var groups = GetAllAssessmentGroups();
            var assessments = GetAllAssessments();
            return (assessments, groups);
        }
    }
}
