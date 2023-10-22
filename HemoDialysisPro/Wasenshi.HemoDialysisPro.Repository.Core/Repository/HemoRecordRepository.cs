using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class HemoRecordRepository : Repository<HemodialysisRecord, Guid>, IHemoRecordRepository
    {
        public HemoRecordRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<HemodialysisRecord> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.DialysisPrescription)
                .AsSingleQuery();
        }

        public IQueryable<HemoRecordResult> GetAllWithPatient(bool includePrescription = true)
        {
            var all = GetAll(includePrescription)
                .Join(context.Patients, record => record.PatientId, patient => patient.Id, (record, patient) => new HemoRecordResult
                {
                    Record = record,
                    Patient = patient,
                    Prescription = record.DialysisPrescription
                })
                .AsSingleQuery();

            return all;
        }

        public IQueryable<HemodialysisRecord> GetAllWithNote(bool includePrescription = true)
        {
            return GetAll(includePrescription)
                        .Include(x => x.Note)
                        .AsSingleQuery();
        }
    }
}
