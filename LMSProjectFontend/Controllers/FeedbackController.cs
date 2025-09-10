using System.Net.Http.Headers;
using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Http;
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
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackController(IHttpClientFactory httpClientFactory, ILogger<FeedbackController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

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

        #region GetAll Feedback
        public async Task<IActionResult> Index()
        {
            try
            {
                AttachToken();
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
            await PopulateDropdowns();  

            if (id == null || id == 0)
            {
                return View(new Feedback());
            }

            try
            {
                AttachToken();
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
                await PopulateDropdowns();
                TempData["Error"] = "Please correct the validation errors.";
                return View("AddEdit", feedback);
            }

            try
            {
                // ✅ Set CreatedAt before serialization
                if (feedback.FeedbackId == 0)
                    feedback.CreatedAt = DateTime.Now;

                var jsonData = JsonConvert.SerializeObject(feedback);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (feedback.FeedbackId == 0)
                {
                    AttachToken();
                    // ✅ Correct API route
                    response = await _client.PostAsync("FeedbackAPI", content);
                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Feedback added successfully!";
                }
                else
                {
                    AttachToken();
                    feedback.CreatedAt = DateTime.Now; // or use UpdatedAt if you add it later
                    response = await _client.PutAsync($"FeedbackAPI/{feedback.FeedbackId}", content);
                    if (response.IsSuccessStatusCode)
                        TempData["Success"] = "Feedback updated successfully!";
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving feedback.");
                await PopulateDropdowns();
                TempData["Error"] = "Failed to save feedback. Please try again.";
                return View("AddEdit", feedback);
            }

            return RedirectToAction("Index", "Feedback");
        }

        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"FeedbackAPI/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Feedback deleted successfully!";
                }
                else
                {
                    TempData["Error"] = $"Failed to delete feedback with ID {id}.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting feedback with ID {id}");
                TempData["Error"] = "An error occurred while deleting feedback.";
            }

            return RedirectToAction("Index","Feedback");
        }

        #endregion

        #region Student Dropdown
        public async Task<List<StudentDropdownDto>> GetStudentsDropdownAsync()
        {
            try
            {
                AttachToken();
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
                AttachToken();
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

        #region PopulateDropdowns
        private async Task PopulateDropdowns()
        {
            // 🔹 Students
            AttachToken();
            var studentResponse = await _client.GetAsync("FeedbackAPI/student-dropdown");
            studentResponse.EnsureSuccessStatusCode();
            var studentJson = await studentResponse.Content.ReadAsStringAsync();
            var students = JsonConvert.DeserializeObject<List<StudentDropdownDto>>(studentJson);
            ViewBag.Students = new SelectList(students, "Id", "Name");

            // 🔹 Courses
            AttachToken();
            var courseResponse = await _client.GetAsync("FeedbackAPI/course-dropdown");
            courseResponse.EnsureSuccessStatusCode();
            var courseJson = await courseResponse.Content.ReadAsStringAsync();
            var courses = JsonConvert.DeserializeObject<List<CourseDropdownDto>>(courseJson);
            ViewBag.Courses = new SelectList(courses, "Id", "Name");
        }
        #endregion
    }
}
