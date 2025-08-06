namespace LMSProjectFontend.Models
{
    public class CourseModel
    {
        public int CourseId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public int? TeacherId { get; set; }

        public DateTime CreatedAt = DateTime.Now;
    }
}
