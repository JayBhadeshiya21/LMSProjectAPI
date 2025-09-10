using System.Net.Http.Headers;
using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LMSProjectFontend.Areas.User.Controllers
{
    [Area("User")]
    public class CourseController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<CourseController> _logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public CourseController(IHttpClientFactory httpClientFactory, ILogger<CourseController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Auth Token
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

        #endregion

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                AttachToken();

                // 1. Get course
                var courseResp = await _client.GetAsync($"CourseAPI/{id}");
                if (!courseResp.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Course not found.";
                    return RedirectToAction("Index");
                }
                var courseJson = await courseResp.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseModel>(courseJson);

                // 2. Get teacher (if not included in CourseAPI response)
                var teacherResp = await _client.GetAsync($"UserAPI/{course.TeacherId}");
                UserModel? teacher = null;
                if (teacherResp.IsSuccessStatusCode)
                {
                    var teacherJson = await teacherResp.Content.ReadAsStringAsync();
                    teacher = JsonConvert.DeserializeObject<UserModel>(teacherJson);
                }

                // 3. Get modules for this course
                var modulesResp = await _client.GetAsync("ModuleAPI/All");
                var modules = new List<ModuleModel>();
                if (modulesResp.IsSuccessStatusCode)
                {
                    var modulesJson = await modulesResp.Content.ReadAsStringAsync();
                    var allModules = JsonConvert.DeserializeObject<List<ModuleModel>>(modulesJson) ?? new List<ModuleModel>();
                    modules = allModules.Where(m => m.CourseId == id)
                                        .OrderBy(m => m.OrderIndex ?? int.MaxValue)
                                        .ToList();
                }

                // 4. Get feedbacks for course
                var feedbackResp = await _client.GetAsync($"FeedbackAPI/by-course/{id}");
                var feedbacks = new List<FeedbackDisplayModel>();
                if (feedbackResp.IsSuccessStatusCode)
                {
                    var fbJson = await feedbackResp.Content.ReadAsStringAsync();
                    var raw = JsonConvert.DeserializeObject<List<FeedbackByCourseDto>>(fbJson) ?? new List<FeedbackByCourseDto>();
                    foreach (var item in raw)
                    {
                        feedbacks.Add(new FeedbackDisplayModel
                        {
                            FeedbackId = item.FeedbackId,
                            Comment = item.Comment,
                            Rating = item.Rating,
                            CreatedAt = item.FeedbackCreatedAt,
                            StudentName = item.Student?.FullName
                        });
                    }
                }

                // 5. Pass to View
                ViewBag.Course = course;
                ViewBag.Teacher = teacher;
                ViewBag.Feedbacks = feedbacks;
                ViewBag.Modules = modules;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading course details for {CourseId}", id);
                TempData["ErrorMessage"] = "Error loading course details.";
                return RedirectToAction("Index");
            }
        }




        #region Enroll
        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            try
            {
                AttachToken();

                // Resolve current student id via API if available
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
                    TempData["ErrorMessage"] = "Unable to determine current student.";
                    return RedirectToAction("Details", new { id = courseId });
                }

                var dto = new EnrollmentDto
                {
                    EnrollmentId = null,
                    CourseId = courseId,
                    StudentId = studentId.Value,
                    EnrolledOn = DateTime.Now
                };

                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync("EnrollmentAPI/save", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Enrolled successfully.";
                }
                else
                {
                    var respText = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to enroll: {response.StatusCode} {respText}";
                }

                return RedirectToAction("Details", new { id = courseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in course {CourseId}", courseId);
                TempData["ErrorMessage"] = "Unexpected error during enrollment.";
                return RedirectToAction("Details", new { id = courseId });
            }
        }
        #endregion

        #region Get All Course
        public async Task<IActionResult> Index()
        {
            try
            {
                // Call API to get all courses
                AttachToken();
                var response = await _client.GetAsync("CourseAPI");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch courses. Status: {StatusCode}", response.StatusCode);
                    TempData["ErrorMessage"] = "Unable to load courses. Please try again later.";
                    return View(new List<CourseModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var courses = JsonConvert.DeserializeObject<List<CourseModel>>(json) ?? new List<CourseModel>();

                // Get teachers for mapping TeacherName
                var teachers = await GetTeachersDropdownAsync();
                foreach (var course in courses)
                {
                    var teacher = teachers.FirstOrDefault(t => t.Id == course.TeacherId);
                    course.TeacherName = teacher?.Name ?? "Unknown Teacher";
                }

                // Add success message if courses are loaded
                if (courses.Any())
                {
                    TempData["SuccessMessage"] = $"Loaded {courses.Count} course(s) successfully.";
                }

                return View(courses);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while fetching courses");
                TempData["ErrorMessage"] = "Network error. Please check your connection and try again.";
                return View(new List<CourseModel>());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing course data from API");
                TempData["ErrorMessage"] = "Error processing course data. Please try again.";
                return View(new List<CourseModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching courses");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return View(new List<CourseModel>());
            }
        }
        #endregion

        #region DropDown
        private async Task<List<TeacherDropdownDto>> GetTeachersDropdownAsync()
        {
            try
            {
                AttachToken();
                var response = await _client.GetAsync("CourseAPI/dropdown");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(json);

                return teachers ?? new List<TeacherDropdownDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher dropdown.");
                return new List<TeacherDropdownDto>();
            }
        }
        #endregion
    }
}
