using LMSProjectAPI;
using System.Text.Json.Serialization;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }
    public string? Comment { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    [JsonIgnore]
    public virtual Course? Course { get; set; }

    [JsonIgnore]
    public virtual User? Student { get; set; }
}
