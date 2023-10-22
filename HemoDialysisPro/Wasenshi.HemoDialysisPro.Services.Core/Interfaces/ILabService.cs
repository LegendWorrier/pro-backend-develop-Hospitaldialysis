using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface ILabService : IApplicationService
    {
        Page<LabExam> GetAllLabExams(int page = 1, int limit = 25,
            Action<IOrderer<LabExam>> orderBy = null,
            Expression<Func<LabExam, bool>> condition = null);
        LabExam GetLabExam(Guid id);
        IEnumerable<LabExam> CreateLabExamBatch(string patientId, DateTime entryTime, List<LabExam> labExams);
        LabExam UpdateLabExam(LabExam labExam);
        bool DeleteLabExam(Guid id);

        LabExamResult GetLabExamByPatientId(string patientId, Expression<Func<LabExam, bool>> prerequisite = null, DateTime? filter = null, DateTime? upperLimit = null);
        Page<LabOverview> GetLabOverview(int page = 1, int limit = 25, Action<IOrderer<LabOverview>> orderBy = null, Expression<Func<LabOverview, bool>> whereCondition = null);

        IEnumerable<LabHemosheet> GetLabHemosheetList();
        void AddOrUpdateLabHemosheet(IEnumerable<LabHemosheet> labHemosheets);
    }
}