using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public partial class RecordService : IRecordService
    {
        private readonly IDialysisRecordRepository dialysisRecordRepo;
        private readonly IRepository<NurseRecord, Guid> nurseRecordRepo;
        private readonly IRepository<DoctorRecord, Guid> doctorRecordRepo;
        private readonly IRepository<ProgressNote, Guid> progressNoteRepo;
        private readonly IExecutionRecordRepository executionRecordRepo;
        private readonly IMedicinePrescriptionRepository medicinePrescriptionRepo;
        private readonly IMedicineRecordProcessor medProcessor;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;
        private readonly ILogger<RecordService> logger;

        public RecordService(
            IDialysisRecordRepository dialysisRecordRepo,
            IRepository<NurseRecord, Guid> nurseRecordRepo,
            IRepository<DoctorRecord, Guid> doctorRecordRepo,
            IRepository<ProgressNote, Guid> progressNoteRepo,
            IExecutionRecordRepository executionRecordRepo,
            IMedicinePrescriptionRepository medicinePrescriptionRepo,
            IMedicineRecordProcessor medProcessor,
            IRedisClient redis,
            IMessageQueueClient message,
            ILogger<RecordService> logger)
        {
            this.dialysisRecordRepo = dialysisRecordRepo;
            this.nurseRecordRepo = nurseRecordRepo;
            this.doctorRecordRepo = doctorRecordRepo;
            this.progressNoteRepo = progressNoteRepo;
            this.executionRecordRepo = executionRecordRepo;
            this.medicinePrescriptionRepo = medicinePrescriptionRepo;
            this.medProcessor = medProcessor;
            this.redis = redis;
            this.message = message;
            this.logger = logger;
        }
    }
}
