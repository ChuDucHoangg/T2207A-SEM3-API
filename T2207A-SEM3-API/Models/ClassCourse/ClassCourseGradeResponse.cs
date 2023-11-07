namespace T2207A_SEM3_API.Models.ClassCourse
{
    public class ClassCourseGradeResponse
    {
        public int ClassCourseId { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        public float FinalScore { get; set; }
        public bool IsPass { get; set; }
    }
}
