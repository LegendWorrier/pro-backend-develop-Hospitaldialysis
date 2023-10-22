using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class AVShunt : EntityBase<Guid>
    {
        [Required]
        public string PatientId { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public DateTime? EndDate { get; set; }

        public CatheterType CatheterType { get; set; }
        public SideEnum Side { get; set; }
        [Required]
        public string ShuntSite { get; set; }
        /// <summary>
        /// Where the patient got this catheterization
        /// </summary>
        public string CatheterizationInstitution { get; set; }

        public string Note { get; set; }
        public string ReasonForDiscontinuation { get; set; }

        //TODO: link to image upload system
        [NotMapped]
        public ICollection<Guid> Photographs { get; set; }
    }
}