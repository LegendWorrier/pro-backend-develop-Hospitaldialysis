using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class AVShuntViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public DateTimeOffset? EstablishedDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public string CatheterType { get; set; }
        public string Side { get; set; }

        public string ShuntSite { get; set; }
        /// <summary>
        /// Where the patient got this catheterization
        /// </summary>
        public string CatheterizationInstitution { get; set; }

        public string Note { get; set; }
        public string ReasonForDiscontinuation { get; set; }

        public IEnumerable<string> Photographs { get; set; }
    }
}