using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LMSProjectAPI;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int? StudentId { get; set; }

    public string? StudentName { get; set; }

    public int? CourseId { get; set; }

    public string? CourseName { get; set; }

    public DateTime? EnrolledOn { get; set; }

    [JsonIgnore]
    public virtual Course? Course { get; set; }

    [JsonIgnore]
    public virtual User? Student { get; set; }
}


public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public DateTime? EnrolledOn { get; set; }
}

public class StudentDropdownDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class CourseDropdownDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}


