using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Ward : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }

        public bool IsICU { get; set; }
    }
}
