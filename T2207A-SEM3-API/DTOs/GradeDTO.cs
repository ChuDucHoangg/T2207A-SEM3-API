namespace T2207A_SEM3_API.DTOs
{
    public class GradeDTO
    {
        public int id { get; set; }

        public int student_id { get; set; }

        public int test_id { get; set; }

        public double? score { get; set; }

        public int status { get; set; }

        public DateTime? finishedAt { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
