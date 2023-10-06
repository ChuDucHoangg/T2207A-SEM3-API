using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Class
{
    public class EditClass
    {
        [Required]
        public int id { get; set; }

        [Required(ErrorMessage = "Please enter fullname")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string name { get; set; }

        [Required(ErrorMessage = "Please enter fullname")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string slug { get; set; }

        [Required(ErrorMessage = "Please enter fullname")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Enter up to 50 characters")]
        public string room { get; set; }

        [Required(ErrorMessage = "Please enter teacher")]
        public int teacher_id { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid creation date")]
        public DateTime createdAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid edit date")]
        public DateTime updatedAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid delete date")]
        public DateTime deletedAt { get; set; }
    }
}
