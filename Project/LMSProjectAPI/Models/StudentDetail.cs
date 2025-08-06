using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LMSProjectAPI;

public partial class StudentDetail
{
    public int UserId { get; set; }

    public string? EnrollmentNumber { get; set; }

    public string? CourseStream { get; set; }
    [JsonIgnore]
    public virtual User? User { get; set; } = null!;
}
