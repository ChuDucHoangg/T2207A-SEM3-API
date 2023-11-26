using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Question
{
    public class QuestionEssayRequest
    {

        [Required(ErrorMessage = "Please enter title")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        public string title { get; set; }

    }
}
