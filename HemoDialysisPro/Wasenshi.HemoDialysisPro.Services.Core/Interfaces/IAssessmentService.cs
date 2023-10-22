using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IAssessmentService : IApplicationService
    {
        IEnumerable<Assessment> GetAllAssessments();
        IEnumerable<AssessmentGroup> GetAllAssessmentGroups();
        Assessment GetAssessment(long id);
        AssessmentGroup GetGroup(int id);
        void RemoveAssessment(long id);
        void AddAssessment(Assessment assessment);
        bool UpdateAssessment(Assessment assessment);
        void ReorderAssessments(long first, long second);

        void AddGroup(AssessmentGroup group);
        bool UpdateGroup(AssessmentGroup group);
        void RemoveGroup(int id);
        void ReorderGroups(int first, int second);

        IEnumerable<AssessmentItem> GetHemosheetAssessmentItems(Guid hemosheetId);
        AssessmentItem GetItem(Guid id);
        int AddOrUpdateItems(Guid hemosheetId, IEnumerable<AssessmentItem> items);
        int AddOrUpdateItemsForDialysisRecord(Guid dialysisRecordId, IEnumerable<DialysisRecordAssessmentItem> items);
        bool RemoveItem(Guid itemId);

        // ============= Root Admin Only ==============
        void ImportAllAssessmentAndGroup(IEnumerable<Assessment> assessments, IEnumerable<AssessmentGroup> groups);
        (IEnumerable<Assessment> assessments, IEnumerable<AssessmentGroup> groups) ExportAllAssessmentAndGroup();
    }
}
