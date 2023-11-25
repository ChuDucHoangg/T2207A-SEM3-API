using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.ClassCourse
{
    public class EditClassCourse
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter start date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? start_date { get; set; }

        [Required(ErrorMessage = "Please enter end date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? end_date { get; set; }
    }
}
