using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSProjectFontend.Models
{
    public class CourseModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Course title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        [Display(Name = "Course Title")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Course description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a teacher")]
        [Display(Name = "Teacher")]
        public int? TeacherId { get; set; }

        public string? TeacherName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // For file upload (not mapped to DB)
        [NotMapped]
        [Display(Name = "Course Image")]
        public IFormFile? CourseImage { get; set; }

        // Stores the relative/absolute URL for frontend display
        public string? ImageUrl { get; set; }

        // Optional: Store file system path (if needed for backend use)
        public string? ImagePath { get; set; }
    }

    public class TeacherDropdownDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }



}
