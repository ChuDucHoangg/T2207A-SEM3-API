namespace T2207A_SEM3_API.DTOs
{
    public class ClassCourseDTO
    {
        public int id { get; set; }

        public string? className { get; set; }

        public string? courseName { get; set; }

        public string? createByName { get; set; }

        public int class_id { get; set; }

        public int course_id { get; set; }

        public int status { get; set; }

        public DateTime? startDate { get; set; }

        public DateTime? endDate { get; set; }

        public int createdBy { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
