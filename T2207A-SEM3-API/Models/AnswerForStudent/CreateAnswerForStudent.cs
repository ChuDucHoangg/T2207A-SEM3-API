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

        [DataType(DataType.Date, ErrorMessage = "Invalid creation date")]
        public DateTime createdAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid edit date")]
        public DateTime updatedAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid delete date")]
        public DateTime deletedAt { get; set; }
    }
}
