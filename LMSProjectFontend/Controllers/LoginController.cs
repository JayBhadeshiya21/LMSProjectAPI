using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LMSProjectFontend;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Net.Http;
using System;

namespace LMSProjectFontend.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;


        // ✅ Inject IHttpContextAccessor and IHttpClientFactory
        public LoginController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string FullName, string Email, string Password)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri("http://localhost:5281/api/");

                var payload = new
                {
                    FullName = FullName,
                    Email = Email,
                    Password = Password,
                    Role = "Student",
                    Status = true,
                    CreatedAt = DateTime.Now
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("UserAPI/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = "Registration failed: " + msg;
                    return View();
                }

                // Auto login after registration
                return await Login(Email, Password, "Student");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred during registration. Please try again.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password, string Role)
        {
            try
            {
                var authService = new AuthService();
                var jsonData = await authService.AuthenticateUserAsync(Email, Password, Role);

                if (jsonData == null)
                {
                    ViewBag.Error = "Invalid credentials.";
                    return View();
                }

                var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonData);
                string token = data.ContainsKey("token") ? data["token"] : null;

                if (string.IsNullOrEmpty(token))
                {
                    ViewBag.Error = "Invalid credentials.";
                    return View();
                }

                // Store user data in session
                _httpContextAccessor.HttpContext.Session.SetString("JWTToken", token);
                _httpContextAccessor.HttpContext.Session.SetString("UserRole", Role);
                _httpContextAccessor.HttpContext.Session.SetString("UserEmail", Email);

                // Debug: Log the role and redirect decision
                Console.WriteLine($"User logged in with role: {Role}");
                Console.WriteLine($"Redirecting to appropriate dashboard...");

                // Normalize role to lowercase for comparison
                var normalizedRole = Role?.ToLower().Trim();

                // Redirect based on role (student -> User dashboard)
                if (normalizedRole == "student")
                {
                    return RedirectToAction("Index", "User", new { area = "User" });
                }
                else if (normalizedRole == "admin")
                {
                    Console.WriteLine("Redirecting admin to Home controller");
                    return RedirectToAction("Index", "Home");
                }
                else if (normalizedRole == "teacher")
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine($"Unknown role: {Role}, redirecting to Home");
                    // Default redirect for other roles
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                ViewBag.Error = "An error occurred during login. Please try again.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Clear session data
            _httpContextAccessor.HttpContext.Session.Clear();
            
            // Redirect to login page
            return RedirectToAction("Login");
        }

    }
}
