namespace T2207A_SEM3_API.DTOs
{
    public class QuestionDTO
    {
        public int id { get; set; }

        public string? title { get; set; }

        public int level { get; set; }

        public int question_type { get; set; }

        public double score { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
