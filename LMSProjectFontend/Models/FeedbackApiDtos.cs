using System;

namespace LMSProjectFontend.Models
{
    public class FeedbackByCourseDto
    {
        public int? FeedbackId { get; set; }
        public string? Comment { get; set; }
        public int? Rating { get; set; }
        public DateTime? FeedbackCreatedAt { get; set; }
        public int? StudentId { get; set; }
        public StudentLiteDto? Student { get; set; }
        public CourseLiteDto? Course { get; set; }
    }

    public class StudentLiteDto
    {
        public int? UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class CourseLiteDto
    {
        public int? CourseId { get; set; }
        public string? Title { get; set; }
    }
}


