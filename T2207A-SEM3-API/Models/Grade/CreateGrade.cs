using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Grade
{
    public class CreateGrade
    {
        [Required(ErrorMessage = "Please enter student")]
        public int student_id { get; set; }

        [Required(ErrorMessage = "Please enter test")]
        public int test_id { get; set; }

        [Required(ErrorMessage = "Please enter score")]
        [Range(0.0, 100.0, ErrorMessage = "Invalid value")]
        public double score { get; set; }

        [Required(ErrorMessage = "Please enter status")]
        [Range(0, 1, ErrorMessage = "Please select a valid status")]
        public int status { get; set; }

        [Required(ErrorMessage = "Please enter time")]
        public double time_taken { get; set; }
    }
}
