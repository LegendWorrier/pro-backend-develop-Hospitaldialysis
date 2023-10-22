using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Tag : EntityBase<Guid>
    {
        [Required]
        public string PatientId { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
        public bool Italic { get; set; }
        public bool Bold { get; set; }
        public string StrikeThroughStyle { get; set; }

        [NotMapped, JsonIgnore, IgnoreDataMember]
        public Patient Patient { get; set; }
    }
}