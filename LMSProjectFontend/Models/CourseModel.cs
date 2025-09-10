using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

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

        // Optional - only needed when displaying teacher details in list/views
        public string? TeacherName { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   

        
        [Display(Name = "Course Image URL")]
        public string? ImageUrl { get; set; }

        // File upload (ignored in DB and JSON)
        [NotMapped]
        [JsonIgnore]
        [Display(Name = "Upload Course Image")]
        public IFormFile? CourseImage { get; set; }
    }


    public class TeacherDropdownDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
