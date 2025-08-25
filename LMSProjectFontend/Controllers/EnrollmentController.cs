using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LMSProjectFontend.Controllers
{
    public class EnrollmentController : Controller
    {
        #region Configuration Fields

        private readonly HttpClient _client;
        private readonly ILogger<EnrollmentController> _logger;
        public EnrollmentController(IHttpClientFactory httpClientFactory, ILogger<EnrollmentController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
        }

        #endregion

        #region GetAll Enrollment
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _client.GetAsync("EnrollmentAPI/All");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<Enrollment>>(json);
                return View(list);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
                TempData["Error"] = "Unable to load course.";
                return View(new List<Enrollment>());
            }
        }
        #endregion

        #region AddAndEdit

        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Enrollment());
            }

            try
            {
                var response = await _client.GetAsync($"EnrollmentAPI/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var enrollment = JsonConvert.DeserializeObject<Enrollment>(json);

                return View(enrollment); // Edit mode
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading enrollment with ID {id}");
                TempData["Error"] = "Unable to load enrollment for editing.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(Enrollment enrollment)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the validation errors.";
                return View("AddEdit", enrollment);
            }

            try
            {
                var jsonData = JsonConvert.SerializeObject(enrollment);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (enrollment.EnrollmentId == 0)
                {
                    enrollment.EnrolledOn = DateTime.Now;
                    response = await _client.PostAsync("EnrollmentAPI/", content);
                }
                else
                {
                  
                    response = await _client.PutAsync($"EnrollmentAPI/{enrollment.EnrollmentId}", content);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving enrollment.");
                TempData["Error"] = "Failed to save enrollment. Please try again.";
                return View("AddEdit", enrollment);
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"EnrollmentAPI/{id}");
                response.EnsureSuccessStatusCode();

                TempData["Success"] = "Enrollment deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with ID {id}.");
                TempData["Error"] = "Unable to delete Enrollment.";
            }
            return RedirectToAction("Index", "Enrollment");
        }
        #endregion


    }
}
