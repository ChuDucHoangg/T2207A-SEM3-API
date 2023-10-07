using System.ComponentModel.DataAnnotations;

namespace T2207A_SEM3_API.Models.Test
{
    public class CreateTest
    {
        [Required(ErrorMessage = "Please enter fullname")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string name { get; set; }

        [Required(ErrorMessage = "Please enter fullname")]
        [MinLength(3, ErrorMessage = "Enter at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Enter up to 255 characters")]
        public string slug { get; set; }

        [Required(ErrorMessage = "Please enter exam")]
        public int exam_id { get; set; }

        [Required(ErrorMessage = "Please enter Student")]
        public int student_id { get; set; }

        [Required(ErrorMessage = "Please enter StartDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime startDate { get; set; }

        [Required(ErrorMessage = "Please enter EndDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime endDate { get; set; }

        [Required(ErrorMessage = "Please enter past marks")]
        [Range(0.0, 100.0, ErrorMessage = "Invalid value")]
        public double past_marks { get; set; }

        [Required(ErrorMessage = "Please enter total marks")]
        [Range(0.0, 100.0, ErrorMessage = "Invalid value")]
        public double total_marks { get; set; }

        [Required(ErrorMessage = "Please enter CreateBy")]
        public int created_by { get; set; }

        [Required(ErrorMessage = "Please enter status")]
        [Range(1, 3, ErrorMessage = "Please select a valid status")]
        public int status { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid creation date")]
        public DateTime createdAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid edit date")]
        public DateTime updatedAt { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Invalid delete date")]
        public DateTime deletedAt { get; set; }
    }
}
