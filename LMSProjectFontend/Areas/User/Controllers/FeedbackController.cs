using System.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LMSProjectFontend.Areas.User.Controllers
{
    [Area("User")]
    public class FeedbackController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<FeedbackController> _logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackController(IHttpClientFactory httpClientFactory, ILogger<FeedbackController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        protected void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _client.DefaultRequestHeaders.Authorization = null;
            }
        }

        // GET: /User/Feedback
        public async Task<IActionResult> Index()
        {
            try
            {
                AttachToken();
                _logger.LogInformation("Calling API: FeedbackAPI/GetAll");
                var response = await _client.GetAsync("FeedbackAPI/GetAll");
                
                _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch feedback. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = $"API Error: {response.StatusCode} - {errorContent}";
                    return View(new List<FeedbackDisplayDto>());
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response JSON: {Json}", json);
                
                List<FeedbackDisplayDto> feedbacks;
                try
                {
                    // Try to deserialize as the new nested structure first
                    feedbacks = JsonConvert.DeserializeObject<List<FeedbackDisplayDto>>(json) ?? new List<FeedbackDisplayDto>();
                    _logger.LogInformation("Successfully deserialized {Count} feedback items with new structure", feedbacks.Count);
                }
                catch (JsonException)
                {
                    // If that fails, try to deserialize as the old flat structure and convert
                    _logger.LogInformation("New structure failed, trying old structure");
                    var oldFeedbacks = JsonConvert.DeserializeObject<List<Feedback>>(json) ?? new List<Feedback>();
                    feedbacks = oldFeedbacks.Select(f => new FeedbackDisplayDto
                    {
                        FeedbackId = f.FeedbackId,
                        Comment = f.Comment,
                        Rating = f.Rating,
                        FeedbackCreatedAt = f.CreatedAt,
                        StudentId = f.StudentId,
                        Student = new StudentDto { FullName = f.StudentName },
                        Course = new CourseDto { Title = f.CourseName, ImageUrl = null },
                        Teacher = new TeacherDto { FullName = "Unknown Teacher" }
                    }).ToList();
                    _logger.LogInformation("Converted {Count} feedback items from old structure", feedbacks.Count);
                }

                // Normalize image URLs and add placeholder for missing ones
                foreach (var f in feedbacks)
                {
                    if (string.IsNullOrWhiteSpace(f.Course.ImageUrl))
                    {
                        // placeholder icon via data uri fallback (keeps client-only)
                        f.Course.ImageUrl = Url.Content("~/assets/img/illustrations/boy-with-laptop-light.png");
                    }
                }

                return View(feedbacks);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while loading feedback index");
                TempData["ErrorMessage"] = $"JSON Error: {jsonEx.Message}";
                return View(new List<FeedbackDisplayDto>());
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while loading feedback index");
                TempData["ErrorMessage"] = $"Network Error: {httpEx.Message}";
                return View(new List<FeedbackDisplayDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while loading feedback index: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Unexpected Error: {ex.Message}";
                return View(new List<FeedbackDisplayDto>());
            }
        }

        // GET: /User/Feedback/AddFeedback
        [HttpGet]
        public async Task<IActionResult> AddFeedback(int courseId)
        {
            try
            {
                AttachToken();
                
                // Get course details
                var courseResponse = await _client.GetAsync($"CourseAPI/GetById/{courseId}");
                if (!courseResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Unable to load course details.";
                    return RedirectToAction("Index", "Course");
                }

                var courseJson = await courseResponse.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseModel>(courseJson);
                
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Course not found.";
                    return RedirectToAction("Index", "Course");
                }

                var model = new FeedbackSubmissionModel
                {
                    CourseId = courseId,
                    CourseTitle = course.Title,
                    CourseDescription = course.Description,
                    CourseImageUrl = course.ImageUrl,
                    TeacherName = course.TeacherName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing feedback form for course {CourseId}", courseId);
                TempData["ErrorMessage"] = "Unable to prepare feedback form.";
                return RedirectToAction("Index", "Course");
            }
        }

        // POST: /User/Feedback/AddFeedback
        [HttpPost]
        public async Task<IActionResult> AddFeedback(FeedbackSubmissionModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return View(model);
                }

                AttachToken();
                
                // Get current user ID
                int? studentId = null;
                try
                {
                    var meResp = await _client.GetAsync("UserAPI/me");
                    if (meResp.IsSuccessStatusCode)
                    {
                        var meJson = await meResp.Content.ReadAsStringAsync();
                        var me = JsonConvert.DeserializeObject<UserModel>(meJson);
                        studentId = me?.UserId;
                    }
                }
                catch { }

                if (!studentId.HasValue)
                {
                    TempData["ErrorMessage"] = "Unable to determine current user.";
                    return View(model);
                }

                var feedback = new Feedback
                {
                    CourseId = model.CourseId,
                    StudentId = studentId.Value,
                    Comment = model.Comment,
                    Rating = model.Rating,
                    CreatedAt = DateTime.Now
                };

                var jsonData = JsonConvert.SerializeObject(feedback);
                var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("FeedbackAPI", content);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thank you! Your feedback has been submitted successfully.";
                    return RedirectToAction("Details", "Course", new { id = model.CourseId });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to submit feedback. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Failed to submit feedback. Please try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback for course {CourseId}", model.CourseId);
                TempData["ErrorMessage"] = "An error occurred while submitting feedback.";
                return View(model);
            }
        }

        // DTO to match the new nested API response structure
        public class FeedbackDisplayDto
        {
            // Feedback Fields
            public int FeedbackId { get; set; }
            public string? Comment { get; set; }
            public int? Rating { get; set; }
            public DateTime? FeedbackCreatedAt { get; set; }
            public int? StudentId { get; set; }
            
            // Nested Objects
            public StudentDto Student { get; set; } = new StudentDto();
            public CourseDto Course { get; set; } = new CourseDto();
            public TeacherDto Teacher { get; set; } = new TeacherDto();
        }

        public class StudentDto
        {
            public int UserId { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class CourseDto
        {
            public int CourseId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public DateTime? CreatedAt { get; set; }
            public int? TeacherId { get; set; }
        }

        public class TeacherDto
        {
            public int UserId { get; set; }
            public string? FullName { get; set; }
            public string? Email { get; set; }
            public string? Role { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
            public string? Qualification { get; set; }
            public int? ExperienceYears { get; set; }
        }

        // Helper DTOs to align with existing admin Feedback controller endpoints
        private class StudentDropdownDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class CourseDropdownDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private async Task<List<StudentDropdownDto>> GetStudentsDropdownAsync()
        {
            try
            {
                AttachToken();
                var response = await _client.GetAsync("FeedbackAPI/student-dropdown");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<StudentDropdownDto>>(json) ?? new List<StudentDropdownDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student dropdown.");
                return new List<StudentDropdownDto>();
            }
        }

        private async Task<List<CourseDropdownDto>> GetCoursesDropdownAsync()
        {
            try
            {
                AttachToken();
                var response = await _client.GetAsync("FeedbackAPI/course-dropdown");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<CourseDropdownDto>>(json) ?? new List<CourseDropdownDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching course dropdown.");
                return new List<CourseDropdownDto>();
            }
        }

        // Model for feedback submission
        public class FeedbackSubmissionModel
        {
            public int CourseId { get; set; }
            public string? CourseTitle { get; set; }
            public string? CourseDescription { get; set; }
            public string? CourseImageUrl { get; set; }
            public string? TeacherName { get; set; }
            
            [Required(ErrorMessage = "Please provide your feedback comment.")]
            [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
            public string? Comment { get; set; }
            
            [Required(ErrorMessage = "Please provide a rating.")]
            [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
            public int Rating { get; set; }
        }
    }
}


