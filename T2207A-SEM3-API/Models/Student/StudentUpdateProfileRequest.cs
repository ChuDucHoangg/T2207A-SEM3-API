using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Student
{
    public class StudentUpdateProfileRequest
    {
        [Required]
        public int id {  get; set; }
        public IFormFile? avatar { get; set; }

        [Required]
        public DateTime birthday { get; set; }

        public string? phone { get; set; }

        public string? gender { get; set; }

        public string? address { get; set; }
    }
}
