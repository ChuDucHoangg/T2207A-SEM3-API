namespace T2207A_SEM3_API.DTOs
{
    public class TestDTO
    {
        public int id { get; set; }

        public string name { get; set; } 

        public string slug { get; set; }

        public int exam_id { get; set; }

        public int student_id { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public double past_marks { get; set; }

        public double total_marks { get; set; }

        public int created_by { get; set; }

        public int status { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
