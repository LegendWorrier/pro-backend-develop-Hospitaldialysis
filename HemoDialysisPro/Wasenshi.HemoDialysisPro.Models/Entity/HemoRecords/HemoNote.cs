using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class HemoNote : EntityBase<Guid>
    {
        public Guid HemoId { get; set; }

        public string Complication { get; set; }

        [NotMapped, IgnoreMember]
        public HemodialysisRecord Hemosheet { get; set;}
    }
}
