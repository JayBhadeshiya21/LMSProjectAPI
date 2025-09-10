using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LMSProjectFontend.Models
{
    public class ModuleModel
    {
        public int ModuleId { get; set; }

        public int? CourseId { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Content")]
        public string? Content { get; set; }

        [Display(Name = "Video URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Order")]
        public int? OrderIndex { get; set; }

        // ✅ Instead of ignoring Course, expose only Title
        public string? courseTitle { get; set; }
    }

}


