namespace T2207A_SEM3_API.DTOs
{
    public class CourseDTO
    {
        public int id { get; set; }

        public string name { get; set; } = null!;

        public string course_code { get; set; } = null!;

        public int class_id { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updateAt { get; set; }

        public DateTime? deleteAt { get; set; }
    }
}
