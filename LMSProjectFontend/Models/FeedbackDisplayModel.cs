using System;

namespace LMSProjectFontend.Models
{
    public class FeedbackDisplayModel
    {
        public int? FeedbackId { get; set; }
        public string? Comment { get; set; }
        public int? Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? StudentName { get; set; }
    }
}


