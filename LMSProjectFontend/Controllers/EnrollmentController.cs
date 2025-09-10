using System.Net.Http.Headers;
using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace LMSProjectFontend.Controllers
{
    public class EnrollmentController : Controller
    {
        #region Configuration Fields

        private readonly HttpClient _client;
        private readonly ILogger<EnrollmentController> _logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public EnrollmentController(IHttpClientFactory httpClientFactory, ILogger<EnrollmentController> logger, IHttpContextAccessor httpContextAccessor)
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

        #region GetAll Enrollment
        public async Task<IActionResult> Index()
        {
            try
            {
                AttachToken();
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
            try
            {
                if (id == null || id == 0)
                {
                    await PopulateDropdowns();
                    return View(new Enrollment());
                }
                AttachToken();
                var response = await _client.GetAsync($"EnrollmentAPI/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var enrollment = JsonConvert.DeserializeObject<Enrollment>(json);

                await PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
                return View(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading enrollment with ID {id}");
                TempData["Error"] = "Unable to load enrollment.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(Enrollment enrollment)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
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

                TempData["Success"] = enrollment.EnrollmentId == 0
                    ? "Enrollment added successfully!"
                    : "Enrollment updated successfully!";

                return RedirectToAction("Index", "Enrollment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving enrollment.");
                await PopulateDropdowns(enrollment.StudentId, enrollment.CourseId);
                TempData["Error"] = "Failed to save enrollment. Please try again.";
                return RedirectToAction("AddEdit", "Enrollment");
            }
        }
        #endregion

        #region Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                AttachToken();
                var response = await _client.DeleteAsync($"EnrollmentAPI/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Enrollment deleted successfully.";
                    return RedirectToAction("Index", "Enrollment");
                }
                else
                {
                    _logger.LogError("Failed to delete enrollment. Status: {StatusCode}", response.StatusCode);
                    TempData["Error"] = "Failed to delete enrollment.";
                    return RedirectToAction("Index", "Enrollment");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting enrollment with ID {id}.");
                TempData["Error"] = "An error occurred while deleting the enrollment.";
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region DropDowns

        private async Task PopulateDropdowns(int? selectedStudentId = null, int? selectedCourseId = null)
        {
            AttachToken();
            var coursesResponse = await _client.GetAsync("EnrollmentAPI/courses-dropdown");
            var studentsResponse = await _client.GetAsync("EnrollmentAPI/student-dropdown");

            coursesResponse.EnsureSuccessStatusCode();
            studentsResponse.EnsureSuccessStatusCode();

            var coursesJson = await coursesResponse.Content.ReadAsStringAsync();
            var studentsJson = await studentsResponse.Content.ReadAsStringAsync();

            var courseList = JsonConvert.DeserializeObject<List<CourseDropdownDto>>(coursesJson) ?? new List<CourseDropdownDto>();
            var studentList = JsonConvert.DeserializeObject<List<StudentDropdownDto>>(studentsJson) ?? new List<StudentDropdownDto>();

            ViewBag.Courses = new SelectList(courseList, "Id", "Name", selectedCourseId);
            ViewBag.Students = new SelectList(studentList, "Id", "Name", selectedStudentId);
        }
        #endregion
    }
}
