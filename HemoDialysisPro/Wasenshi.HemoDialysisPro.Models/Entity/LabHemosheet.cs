using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class LabHemosheet
    {
        [Key]
        public int LabItemId { get; set; }
        [Required]
        public string Name { get; set; }

        public bool OnlyOnDate { get; set; }


        [JsonIgnore, NotMapped]
        public LabExamItem Item { get; set; }
    }
}
