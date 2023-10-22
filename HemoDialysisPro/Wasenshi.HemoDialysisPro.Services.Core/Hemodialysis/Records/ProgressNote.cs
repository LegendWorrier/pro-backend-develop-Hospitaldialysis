using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services
{
    public partial class RecordService : IRecordService
    {
        // ================================ Progress Note ==============================================



        public IEnumerable<ProgressNote> GetProgressNotesByHemoId(Guid hemoId)
        {
            return progressNoteRepo.GetAll(false)
                .Where(x => x.HemodialysisId == hemoId)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Focus)
                .ToList();
        }

        public ProgressNote GetProgressNote(Guid id)
        {
            var result = progressNoteRepo.Get(id);
            return result;
        }

        public ProgressNote CreateProgressNote(ProgressNote record)
        {
            progressNoteRepo.Insert(record);
            progressNoteRepo.Complete();

            return record;
        }

        public bool UpdateProgressNote(ProgressNote record)
        {
            var old = progressNoteRepo.Get(record.Id);

            // Safe guard, cannot edit hemoId
            if (old.HemodialysisId != record.HemodialysisId)
            {
                throw new InvalidOperationException("Cannot edit hemosheet id");
            }

            progressNoteRepo.Update(record);

            return progressNoteRepo.Complete() > 0;
        }

        public bool DeleteProgressNote(Guid id)
        {
            progressNoteRepo.Delete(new ProgressNote { Id = id });

            return progressNoteRepo.Complete() > 0;
        }

        public bool SwapProgressNoteOrder(Guid firstId, Guid secondId)
        {
            var first = progressNoteRepo.Get(firstId);
            var second = progressNoteRepo.Get(secondId);

            if (first == null || second == null)
            {
                return false;
            }

            if (first.HemodialysisId != second.HemodialysisId)
            {
                throw new InvalidOperationException();
            }

            (second.Order, first.Order) = (first.Order, second.Order);
            if (first.Order == second.Order)
            {
                first.Order++; // try to remedy
            }
            progressNoteRepo.Update(first);
            progressNoteRepo.Update(second);

            return progressNoteRepo.Complete() == 2;
        }
    }
}
