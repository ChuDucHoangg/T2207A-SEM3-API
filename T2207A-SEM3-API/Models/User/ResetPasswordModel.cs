using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.User
{
    public class ResetPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)] // Độ dài tối thiểu của mật khẩu
        public string NewPassword { get; set; }
    }
}
