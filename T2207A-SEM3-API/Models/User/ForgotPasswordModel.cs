using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.User
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }
    }
}
