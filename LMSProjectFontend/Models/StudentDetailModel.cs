using System.ComponentModel.DataAnnotations;

namespace LMSProjectFontend.Models
{
    public class StudentDetailModel
    {
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Enrollment Number")]
        public string? EnrollmentNumber { get; set; }

        [Required]
        [Display(Name = "Course / Stream")]
        public string? CourseStream { get; set; }
    }
}



