using System.ComponentModel.DataAnnotations;
using T2207A_SEM3_API.Models.Answer;

namespace T2207A_SEM3_API.Models.Question
{
    public class QuestionMultipleChoiceRequest
    {
        [Required(ErrorMessage = "Please enter title")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string title { get; set; }

        [Required(ErrorMessage = "Please enter level")]
        [Range(1, 3, ErrorMessage = "Please select a valid level")]
        public int level { get; set; }


        [Required]
        public List<AnswerRequest> answers { get; set; }
    }
}
