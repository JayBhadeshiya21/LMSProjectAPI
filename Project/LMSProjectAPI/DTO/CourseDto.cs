namespace LMSProjectAPI.DTO
{
	public class CourseDto
	{
		public string? Title { get; set; }
		public string? Description { get; set; }
		public int? TeacherId { get; set; }
		public DateTime? CreatedAt { get; set; }

		[NotMapped]
		public IFormFile? Image { get; set; }

		public string? ImagePath { get; set; }
	}
}
