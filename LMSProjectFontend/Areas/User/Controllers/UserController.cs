using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using LMSProjectFontend.Models;

namespace LMSProjectFontend.Areas.User.Controllers
{
    [Area("User")]
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public UserController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://localhost:5281/api/");
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in and is a student
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token) || userRole?.ToLower() != "student")
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            try
            {
                // Attach JWT token to request
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Fetch dashboard summary data
                var summaryResponse = await _httpClient.GetAsync("UserAPI/Summary");
                if (summaryResponse.IsSuccessStatusCode)
                {
                    var summaryJson = await summaryResponse.Content.ReadAsStringAsync();
                    var summary = JsonConvert.DeserializeObject<DashboardSummaryModel>(summaryJson);
                    ViewBag.DashboardSummary = summary;
                }
                else
                {
                    // Default fallback if API fails
                    ViewBag.DashboardSummary = new DashboardSummaryModel();
                }
            }
            catch (Exception ex)
            {
                // Fallback on error
                ViewBag.DashboardSummary = new DashboardSummaryModel();
            }

            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            // Check if user is logged in and is a student
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token) || userRole?.ToLower() != "student")
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            try
            {
                // Attach JWT token to request
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Fetch dashboard summary data
                var summaryResponse = await _httpClient.GetAsync("UserAPI/Summary");
                if (summaryResponse.IsSuccessStatusCode)
                {
                    var summaryJson = await summaryResponse.Content.ReadAsStringAsync();
                    var summary = JsonConvert.DeserializeObject<DashboardSummaryModel>(summaryJson);
                    ViewBag.DashboardSummary = summary;
                }
                else
                {
                    // Default fallback if API fails
                    ViewBag.DashboardSummary = new DashboardSummaryModel();
                }
            }
            catch (Exception ex)
            {
                // Fallback on error
                ViewBag.DashboardSummary = new DashboardSummaryModel();
            }

            return View();
        }

        public async Task<IActionResult> Profile()
        {
            // Check if user is logged in and is a student
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token) || userRole?.ToLower() != "student")
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Get current user
                var meResponse = await _httpClient.GetAsync("UserAPI/me");
                if (!meResponse.IsSuccessStatusCode)
                {
                    return View(new ProfileViewModel());
                }
                var meJson = await meResponse.Content.ReadAsStringAsync();
                var me = JsonConvert.DeserializeObject<UserModel>(meJson);

                // Sync session role/email in case they changed server-side
                if (!string.IsNullOrWhiteSpace(me?.Role))
                {
                    _httpContextAccessor.HttpContext.Session.SetString("UserRole", me.Role);
                }
                if (!string.IsNullOrWhiteSpace(me?.Email))
                {
                    _httpContextAccessor.HttpContext.Session.SetString("UserEmail", me.Email);
                }

                // Try load student details
                string? enrollment = null;
                string? stream = null;
                try
                {
                    var sdResp = await _httpClient.GetAsync($"StudentDetailAPI/{me.UserId}");
                    if (sdResp.IsSuccessStatusCode)
                    {
                        var sdJson = await sdResp.Content.ReadAsStringAsync();
                        var sd = JsonConvert.DeserializeObject<dynamic>(sdJson);
                        enrollment = (string?)sd?.EnrollmentNumber;
                        stream = (string?)sd?.CourseStream;
                    }
                }
                catch { }

                var vm = new ProfileViewModel
                {
                    UserId = me.UserId,
                    FullName = me.FullName,
                    Email = me.Email,
                    Role = me.Role,
                    EnrollmentNumber = enrollment,
                    CourseStream = stream
                };

                return View(vm);
            }
            catch
            {
                return View(new ProfileViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> RegisterDetails(bool? cleared)
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
            if (string.IsNullOrEmpty(token) || userRole?.ToLower() != "student")
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var meResponse = await _httpClient.GetAsync("UserAPI/me");
                if (!meResponse.IsSuccessStatusCode)
                {
                    return View(new StudentDetailModel());
                }
                var meJson = await meResponse.Content.ReadAsStringAsync();
                var me = JsonConvert.DeserializeObject<UserModel>(meJson);

                var model = new StudentDetailModel { UserId = me.UserId };

                // If redirected after successful update with cleared flag, return empty fields
                if (cleared == true)
                {
                    return View(model);
                }

                var sdResp = await _httpClient.GetAsync($"StudentDetailAPI/{me.UserId}");
                if (sdResp.IsSuccessStatusCode)
                {
                    var sdJson = await sdResp.Content.ReadAsStringAsync();
                    var sd = JsonConvert.DeserializeObject<dynamic>(sdJson);
                    model.EnrollmentNumber = (string?)sd?.EnrollmentNumber;
                    model.CourseStream = (string?)sd?.CourseStream;
                }

                return View(model);
            }
            catch
            {
                return View(new StudentDetailModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDetails(StudentDetailModel model)
        {
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
            var email = _httpContextAccessor.HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(token) || userRole?.ToLower() != "student")
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Find current user id
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var meResponse = await _httpClient.GetAsync("UserAPI/me");
                if (!meResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Could not resolve current user.";
                    return View(model);
                }
                var meJson = await meResponse.Content.ReadAsStringAsync();
                var me = JsonConvert.DeserializeObject<UserModel>(meJson);

                var payload = new
                {
                    UserId = me.UserId,
                    EnrollmentNumber = model.EnrollmentNumber,
                    CourseStream = model.CourseStream
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var resp = await _httpClient.PostAsync("StudentDetailAPI", content);
                if (!resp.IsSuccessStatusCode)
                {
                    var msg = await resp.Content.ReadAsStringAsync();
                    ViewBag.Error = "Save failed: " + msg;
                    return View(model);
                }

                TempData["SuccessMessage"] = "Student details updated successfully.";
                return RedirectToAction("RegisterDetails", new { cleared = true });
            }
            catch (Exception)
            {
                ViewBag.Error = "Unexpected error while saving details.";
                return View(model);
            }
        }

        public IActionResult Courses()
        {
            // Check if user is logged in and is a student
            var userRole = _httpContextAccessor.HttpContext.Session.GetString("UserRole");
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token) || userRole?.ToLower() != "student")
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            return View();
        }
    }
}
