using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace LMSProjectFontend.Controllers
{
    public class FeedbackController : Controller
    {
        #region Configuration Fields

        private readonly HttpClient _client;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(IHttpClientFactory httpClientFactory, ILogger<FeedbackController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
        }

        #endregion

        #region GetAll Feedback

        public async Task<IActionResult> Index()
        {
            try
            {
                // 🔹 Get courses (dropdown API)
                var courseResponse = await _client.GetAsync("CourseAPI/course-dropdown");
                courseResponse.EnsureSuccessStatusCode();
                var courseJson = await courseResponse.Content.ReadAsStringAsync();
                var courses = JsonConvert.DeserializeObject<List<CourseDropdownDto>>(courseJson);

                // 🔹 Get students (dropdown API)
                var studentResponse = await _client.GetAsync("CourseAPI/student-dropdown");
                studentResponse.EnsureSuccessStatusCode();
                var studentJson = await studentResponse.Content.ReadAsStringAsync();
                var students = JsonConvert.DeserializeObject<List<StudentDropdownDto>>(studentJson);

                // 🔹 Pass to ViewBag
                ViewBag.Teachers = new SelectList(courses, "Id", "Name");
                ViewBag.Students = new SelectList(students, "Id", "Name");

                var response = await _client.GetAsync("FeedbackAPI/All");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<Feedback>>(json);
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching feedback.");
                TempData["Error"] = "Unable to load feedback.";
                return View(new List<Feedback>());
            }
        }

        #endregion

        #region AddAndEdit

        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Feedback());
            }

            try
            {
                var response = await _client.GetAsync($"FeedbackAPI/{id}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var feedback = JsonConvert.DeserializeObject<Feedback>(json);
                return View(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading feedback with ID {id}");
                TempData["Error"] = "Unable to load feedback for editing.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(Feedback feedback)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the validation errors.";
                return View("AddEdit", feedback);
            }

            try
            {
                var jsonData = JsonConvert.SerializeObject(feedback);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (feedback.FeedbackId == 0)
                {
                    feedback.CreatedAt = DateTime.Now;
                    response = await _client.PostAsync("FeedbackAPI/", content);
                }
                else
                {
                    feedback.CreatedAt = DateTime.Now;
                    response = await _client.PutAsync($"FeedbackAPI/{feedback.FeedbackId}", content);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving feedback.");
                TempData["Error"] = "Failed to save feedback. Please try again.";
                return View("AddEdit", feedback);
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Student Dropdown
        public async Task<List<StudentDropdownDto>> GetStudentsDropdownAsync()
        {
            try
            {
                var response = await _client.GetAsync("FeedbackAPI/student-dropdown");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var students = JsonConvert.DeserializeObject<List<StudentDropdownDto>>(json);

                return students ?? new List<StudentDropdownDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching student dropdown.");
                return new List<StudentDropdownDto>();
            }
        }
        #endregion

        #region Course Dropdown
        public async Task<List<CourseDropdownDto>> GetCoursesDropdownAsync()
        {
            try
            {
                var response = await _client.GetAsync("FeedbackAPI/course-dropdown"); // Course API endpoint
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var courses = JsonConvert.DeserializeObject<List<CourseDropdownDto>>(json);

                return courses ?? new List<CourseDropdownDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching course dropdown.");
                return new List<CourseDropdownDto>();
            }
        }
        #endregion


    }
}
