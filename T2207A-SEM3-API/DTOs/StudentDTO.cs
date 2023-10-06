namespace T2207A_SEM3_API.DTOs
{
    public class StudentDTO
    {
        public int id { get; set; }

        public string student_code { get; set; } = null!;

        public string fullname { get; set; } = null!;

        public DateTime birthday { get; set; }

        public string email { get; set; } = null!;

        public string phone { get; set; } = null!;

        public int class_id { get; set; }

        public string password { get; set; } = null!;

        public int role { get; set; }

        public int status { get; set; }

        public DateTime createdAt { get; set; }

        public DateTime updateAt { get; set; }

        public DateTime deleteAt { get; set; }
    }
}
