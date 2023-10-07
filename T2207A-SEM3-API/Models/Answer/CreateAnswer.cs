using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Answer
{
    public class CreateAnswer
    {
        [Required(ErrorMessage = "Please enter content")]
        public string content { get; set; }

        [Required(ErrorMessage = "Please enter status")]
        [Range(0, 1, ErrorMessage = "Please select a valid status")]
        public int status { get; set; }

        [Required(ErrorMessage = "Please enter question")]
        public int question_id { get; set; }

    }
}
