using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace LMSProjectFontend.Models
{
    public partial class Enrollment
    {
        public int EnrollmentId { get; set; }

        public int? StudentId { get; set; }

        public string? studentName { get; set; }

        public int? CourseId { get; set; }

        public string? courseName { get; set; }

        public DateTime EnrolledOn { get; set; }
    }
}

