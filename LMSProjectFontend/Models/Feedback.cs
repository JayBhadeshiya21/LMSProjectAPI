using LMSProjectFontend;
using System.Text.Json.Serialization;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? StudentId { get; set; }

    public string? studentName { get; set; }

    public int? CourseId { get; set; }

    public string? courseName { get; set; }
    public string? comments { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }
}

