using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LMSProjectAPI;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }
    [Required]
    public string? Comment { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    [JsonIgnore]
    public virtual Course? Course { get; set; }

    [JsonIgnore]
    public virtual User? Student { get; set; }
}

public class FeedbackDto
{
    public int? FeedbackId { get; set; }
    public string? Comments { get; set; }
    public int? Rating { get; set; }
    public int? CourseId { get; set; }
    
    public string? CourseName { get; set; }
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string ? ImageUrl { get; set; }
}
