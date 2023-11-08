using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Answer
{
    public class EditAnswer
    {
        [Required]
        public int id { get; set; }

        [Required(ErrorMessage = "Please enter content")]
        public string content { get; set; }
    }
}
