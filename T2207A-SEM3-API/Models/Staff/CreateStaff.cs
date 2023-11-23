using System.ComponentModel.DataAnnotations;
namespace T2207A_SEM3_API.Models.Staff
{
    public class CreateStaff
    {

        [Required(ErrorMessage = "Please enter avatar")]
        public IFormFile avatar { get; set; }

        [Required(ErrorMessage = "Please enter fullname")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string fullname { get; set; }

        [Required(ErrorMessage = "Please enter birthday")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime birthday { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [RegularExpression(@"^\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}\b", ErrorMessage = "Email address is not valid")]
        public string email { get; set; }

        [Required(ErrorMessage = "Please enter gender")]
        public string gender { get; set; }

        [Required(ErrorMessage = "Please enter address")]
        public string address { get; set; }

        [Required(ErrorMessage = "Please enter phone")]
        [MinLength(10, ErrorMessage = "Enter at least 10 characters")]
        [MaxLength(12, ErrorMessage = "Enter up to 12 characters")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Please enter role")]
        public string role { get; set; }
    }
}
