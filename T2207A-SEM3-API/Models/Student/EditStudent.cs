using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Student
{
    public class EditStudent
    {
        [Required]
        public int id { get; set; }

        [Required(ErrorMessage = "Please enter student code")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(100, ErrorMessage = "Enter up to 100 characters")]
        public string student_code { get; set; }

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

        [Required(ErrorMessage = "Please enter phone")]
        [MinLength(10, ErrorMessage = "Enter at least 10 characters")]
        [MaxLength(12, ErrorMessage = "Enter up to 12 characters")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Please enter class")]
        public int class_id { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [MinLength(6, ErrorMessage = "Enter at least 6 characters")]
        [MaxLength(50, ErrorMessage = "Enter up to 255 characters")]
        public string password { get; set; }

        [Required(ErrorMessage = "Please enter role")]
        [Range(1, 3, ErrorMessage = "Please select a valid role")]
        public int role { get; set; }

        [Required(ErrorMessage = "Please enter status")]
        [Range(1, 3, ErrorMessage = "Please select a valid role")]
        public int status { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid creation date")]
        public DateTime createdAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid edit date")]
        public DateTime updatedAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid delete date")]
        public DateTime deletedAt { get; set; }
    }
}
