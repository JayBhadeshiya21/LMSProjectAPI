namespace LMSProjectFontend.Areas.User.Models
{
    public class UserDashboardModel
    {
        public int ActiveCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int TotalCertificates { get; set; }
        public double CompletionRate { get; set; }
        public int StudyHours { get; set; }
        public List<UserCourseModel> EnrolledCourses { get; set; } = new List<UserCourseModel>();
    }

    public class UserCourseModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public double ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty; // "In Progress", "Completed", "Not Started"
        public DateTime EnrolledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}

