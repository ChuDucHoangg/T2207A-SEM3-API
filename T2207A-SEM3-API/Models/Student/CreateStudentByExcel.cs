using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Student
{
    public class CreateStudentByExcel
    {
        [Required(ErrorMessage = "Please enter file excel")]
        public IFormFile excelFile { get; set; }

        [Required(ErrorMessage = "Please enter class")]
        public int class_id { get; set; }
    }
}
