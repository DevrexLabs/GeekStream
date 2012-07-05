using System.ComponentModel.DataAnnotations;

namespace GeekStream.Models
{
    public class AddFeedModel
    {
        [Required]
        public string Feed { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}