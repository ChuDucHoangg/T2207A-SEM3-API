using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Question
{
    public class CreateQuestionEssay
    {
        [Required(ErrorMessage = "Please enter title")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string title { get; set; }


        [Required(ErrorMessage = "Please enter course")]
        public int course_id { get; set; }
    }
}
