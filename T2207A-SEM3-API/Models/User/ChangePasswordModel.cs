using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.User
{
    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters, dude!")]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; } 
    }
}
