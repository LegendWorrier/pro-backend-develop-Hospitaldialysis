using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class AvShuntService : IAvShuntService
    {
        private readonly IAvShuntRepository avShuntRepo;
        private readonly IRepository<AVShuntIssueTreatment, Guid> avShuntIssueRepo;

        public AvShuntService(
            IAvShuntRepository avShuntRepo,
            IRepository<AVShuntIssueTreatment, Guid> avShuntIssueRepo)
        {
            this.avShuntRepo = avShuntRepo;
            this.avShuntIssueRepo = avShuntIssueRepo;
        }

        public AVResult GetAvViewResultByPatientId(string patientId)
        {
            var avShunts = avShuntRepo.Find(x => x.PatientId == patientId)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.EstablishedDate)
                .ThenBy(x => x.ShuntSite)
                .ThenBy(x => x.Side);
            var avIssues = avShuntIssueRepo.Find(x => x.PatientId == patientId)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.AbnormalDatetime);
            return new AVResult
            {
                AvShunts = avShunts,
                AvShuntIssueTreatments = avIssues
            };
        }

        public IEnumerable<AVShunt> GetAvListByPatientId(string patientId)
        {
            return avShuntRepo.GetAll()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.EstablishedDate)
                .ThenBy(x => x.ShuntSite)
                .ThenBy(x => x.Side)
                .ToList();
        }

        public AVShunt GetAvShunt(Guid id)
        {
            var result = avShuntRepo.Get(id);
            return result;
        }

        public AVShunt CreateAvShunt(AVShunt avShunt)
        {
            avShuntRepo.Insert(avShunt);
            avShuntRepo.Complete();

            return avShunt;
        }

        public bool EditAvShunt(AVShunt avShunt)
        {
            var old = avShuntRepo.Get(avShunt.Id);

            // Safe guard, cannot edit patientId!
            if (old.PatientId != avShunt.PatientId)
            {
                throw new InvalidOperationException("Cannot edit patient Id.");
            }

            avShuntRepo.Update(avShunt);

            return avShuntRepo.Complete() > 0;
        }

        public bool DeleteAvShunt(Guid id)
        {
            var avShunt = avShuntRepo.Get(id);
            if (avShunt != null)
            {
                avShunt.IsActive = false;
                avShuntRepo.Update(avShunt);
            }

            return avShuntRepo.Complete() > 0;
        }

        public AVShuntIssueTreatment GetIssueTreatment(Guid id)
        {
            var result = avShuntIssueRepo.Get(id);
            return result;
        }

        public AVShuntIssueTreatment CreateIssueTreatment(AVShuntIssueTreatment issueTreatment)
        {
            avShuntIssueRepo.Insert(issueTreatment);
            avShuntIssueRepo.Complete();

            return issueTreatment;
        }

        public bool EditIssueTreatment(AVShuntIssueTreatment issueTreatment)
        {
            var old = avShuntIssueRepo.Get(issueTreatment.Id);

            // Safe guard, cannot edit patientId!
            if (old.PatientId != issueTreatment.PatientId)
            {
                throw new InvalidOperationException("Cannot edit patient Id.");
            }

            avShuntIssueRepo.Update(issueTreatment);

            return avShuntIssueRepo.Complete() > 0;
        }

        public bool DeleteIssueTreatment(Guid id)
        {
            var issue = avShuntIssueRepo.Get(id);
            if (issue != null)
            {
                issue.IsActive = false;
                avShuntIssueRepo.Update(issue);
            }

            return avShuntIssueRepo.Complete() > 0;
        }
    }
}
