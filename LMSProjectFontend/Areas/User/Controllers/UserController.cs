using Microsoft.AspNetCore.Mvc;

namespace LMSProjectFontend.Areas.User.Controllers
{
    [Area("User")]
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
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

        public IActionResult Dashboard()
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

        public IActionResult Profile()
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
