using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class MedicineResult
    {
        public bool IsSuccess => !Errors.Any();
        public IEnumerable<MedicineRecord> Records { get; set; }
        public List<MedicineError> Errors { get; set; } = new List<MedicineError>();

        public void AddError(Guid prescriptionId, string errorMsg)
        {
            Errors.Add(new MedicineError
            {
                PrescriptionId = prescriptionId,
                ErrorMessage = errorMsg
            });
        }
    }

    public class MedicineError
    {
        public Guid PrescriptionId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
