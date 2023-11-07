namespace T2207A_SEM3_API.Models.Student
{
    public class StudentProfileRespone
    {
        public int id { get; set; }

        public string student_code { get; set; }

        public string fullname { get; set; }

        public string avatar { get; set; }

        public DateTime birthday { get; set; }

        public string email { get; set; }

        public string? phone { get; set; }

        public string? gender { get; set; }

        public string? address { get; set; }

        public string className { get; set; }
    }
}
