using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace Wasenshi.HemoDialysisPro.Web.Api.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName
        {
            get;
            set;
        }
        [Required]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password
        {
            get;
            set;
        }

        public string ReturnUrl
        {
            get;
            set;
        }
    }
}
