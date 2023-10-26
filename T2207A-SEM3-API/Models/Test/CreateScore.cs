using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Test
{
    public class CreateScore
    {
        [Required(ErrorMessage = "Please enter test")]
        public int test_id { get; set; }

        [Required(ErrorMessage = "Please enter student")]
        public int student_id { get; set; }

        [Required(ErrorMessage = "Please enter score")]
        [Range(0.0, 100.0, ErrorMessage = "Invalid value")]
        public int score { get; set; }
    }
}
