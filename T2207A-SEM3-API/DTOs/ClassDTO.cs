namespace T2207A_SEM3_API.DTOs
{
    public class ClassDTO
    {
        public int id { get; set; }

        public string name { get; set; }

        public string slug { get; set; }

        public string? room { get; set; }

        public int teacher_id { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public DateTime? deletedAt { get; set; }
    }
}
