using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Grade
{
    public class EditGrade
    {
        [Required]  
        public int id { get; set; }

        [Required(ErrorMessage = "Please enter score")]
        [Range(0.0, 100.0, ErrorMessage = "Invalid value")]
        public double score { get; set; }

        [Required(ErrorMessage = "Please enter status")]
        [Range(0, 1, ErrorMessage = "Please select a valid status")]
        public int status { get; set; }

    }
}
