using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using LMSProjectFontend;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    [Required(ErrorMessage = "Student ID is required.")]
    public int? StudentId { get; set; }

    [NotMapped]
    public string? StudentName { get; set; }   // ✅ Display only, not mapped

    [Required(ErrorMessage = "Course ID is required.")]
    public int? CourseId { get; set; }

    [NotMapped]
    public string? CourseName { get; set; }    // ✅ Display only, not mapped

    [Required(ErrorMessage = "Comment is required.")]
    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public string? Comment { get; set; }

    [Required(ErrorMessage = "Rating is required.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }
}


