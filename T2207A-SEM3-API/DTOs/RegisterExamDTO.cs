namespace T2207A_SEM3_API.DTOs
{
    public class RegisterExamDTO
    {
        public int id { get; set; }

        public int student_id { get; set; }

        public int exam_id { get; set; }

        public int status { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
