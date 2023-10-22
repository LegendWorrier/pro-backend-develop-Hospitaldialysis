using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Needle : EntityBase<int>
    {
        [Required]
        public int Number { get; set; }
    }
}
