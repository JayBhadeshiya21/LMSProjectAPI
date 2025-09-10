using System;

namespace LMSProjectFontend.Models
{
    public class EnrollmentDto
    {
        public int? EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public DateTime? EnrolledOn { get; set; }
    }
}


