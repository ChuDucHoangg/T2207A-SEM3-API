using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.ClassCourse
{
    public class CreateClassCourse
    {
        [Required(ErrorMessage = "Please enter class")]
        public int class_id { get; set; }

        [Required(ErrorMessage = "Please enter course")]
        public int course_id { get; set; }


        [Required(ErrorMessage = "Please enter start date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? start_date { get; set; }

        [Required(ErrorMessage = "Please enter end date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? end_date { get; set; }

        [Required(ErrorMessage = "Please enter staff")]
        public int created_by { get; set; }
    }
}
