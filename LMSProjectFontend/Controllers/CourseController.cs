using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LMSProjectFontend.Controllers
{
    public class CourseController : Controller
    {
        #region Configuration Fields

        private readonly HttpClient _client;
        private readonly ILogger<CourseController> _logger;
        public CourseController(IHttpClientFactory httpClientFactory, ILogger<CourseController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
        }

        #endregion

        #region GetAll Course
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _client.GetAsync("CourseAPI");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<CourseModel>>(json);
                return View(list);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
                TempData["Error"] = "Unable to load course.";
                return View(new List<CourseModel>());
            }
        }
        #endregion

        #region AddAndEdit

        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new CourseModel());
            }

            try
            {
                var response = await _client.GetAsync($"CourseAPI/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseModel>(json);

                return View(course); // Edit mode
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading course with ID {id}");
                TempData["Error"] = "Unable to load course for editing.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(CourseModel course)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the validation errors.";
                return View(course.CourseId == 0 ? "AddEdit" : "AddEdit", course);
            }

            try
            {
                var jsonData = JsonConvert.SerializeObject(course);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (course.CourseId == 0)
                {
                    course.CreatedAt = DateTime.Now;
                    response = await _client.PostAsync("CourseAPI/", content);
                }
                else
                {
                    response = await _client.PutAsync($"CourseAPI/{course.CourseId}", content);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving course.");
                TempData["Error"] = "Failed to save course. Please try again.";
                return View("AddEdit", course);
            }

            return RedirectToAction("Index");
        }

        #endregion

    }
}

