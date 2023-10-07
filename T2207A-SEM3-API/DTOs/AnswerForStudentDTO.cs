namespace T2207A_SEM3_API.DTOs
{
    public class AnswerForStudentDTO
    {
        public int id { get; set; }

        public int question_id { get; set; }

        public string content { get; set; }

        public int student_id { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
