namespace T2207A_SEM3_API.Models.ClassCourse
{
    public class ClassCourseResponse
    {
        public int classCourseId { get; set; }
        public string className { get; set; }

        public string courseName { get; set; }

        public string createByName { get; set; }

        public int numberOfStudents { get; set; }

        public DateTime? startDate { get; set; }

        public DateTime? endDate { get; set; }
    }
}
