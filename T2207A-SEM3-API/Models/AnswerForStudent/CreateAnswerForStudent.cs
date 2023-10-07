using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.AnswerForStudent
{
    public class CreateAnswerForStudent
    {
        [Required(ErrorMessage = "Please enter question")]
        public int question_id { get; set; }

        [Required(ErrorMessage = "Please enter content")]
        public string content { get; set; }

        [Required(ErrorMessage = "Please enter student")]
        public int student_id { get; set; }

    }
}
