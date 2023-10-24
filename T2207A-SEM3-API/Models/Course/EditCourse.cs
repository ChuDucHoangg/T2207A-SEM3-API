using System.ComponentModel.DataAnnotations;
namespace T2207A_SEM3_API.Models.Course
{
    public class EditCourse
    {
        [Required]
        public int id { get; set; }

        [Required(ErrorMessage = "Please enter name course")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string name { get; set; }

        [Required(ErrorMessage = "Please enter course code")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(100, ErrorMessage = "Enter up to 100 characters")]
        public string course_code { get; set; }

        [Required(ErrorMessage = "Please enter class")]
        public int class_id { get; set; }


    }
}

