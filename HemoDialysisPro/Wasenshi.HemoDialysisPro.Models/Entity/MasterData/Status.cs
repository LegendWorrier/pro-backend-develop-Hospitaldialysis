using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Status : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StatusCategories Category { get; set; }
    }
}