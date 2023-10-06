namespace T2207A_SEM3_API.DTOs
{
    public class ExamDTO
    {
        public int id { get; set; }

        public string name { get; set; }

        public string slug { get; set; }

        public int course_id { get; set; }

        public DateTime start_date { get; set; }

        public int? created_by { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
