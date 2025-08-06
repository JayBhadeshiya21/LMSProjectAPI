using LMSProjectAPI;
using System.Text.Json.Serialization;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool Status { get; set; } = true;

    [JsonIgnore]
    public virtual TeacherDetail? TeacherDetail { get; set; }

    [JsonIgnore]
    public virtual StudentDetail? StudentDetail { get; set; }

    [JsonIgnore]
    public virtual ICollection<Course>? Courses { get; set; }

    [JsonIgnore]
    public virtual ICollection<Enrollment>? Enrollments { get; set; }

    [JsonIgnore]
    public virtual ICollection<Feedback>? Feedbacks { get; set; }
}
