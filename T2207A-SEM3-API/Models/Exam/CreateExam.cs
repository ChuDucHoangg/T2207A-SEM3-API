using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Exam
{
    public class CreateExam
    {
        [Required(ErrorMessage = "Please enter name")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string name { get; set; }

        [Required(ErrorMessage = "Please enter teacher")]
        public int courseClass_id { get; set; }

        [Required(ErrorMessage = "Please enter start date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime start_date { get; set; }

        [Required(ErrorMessage = "Please enter teacher")]
        public int created_by { get; set; }
        
    }
}
