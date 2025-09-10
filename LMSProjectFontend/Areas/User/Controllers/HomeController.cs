using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LMSProjectFontend.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult About()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
            var role = _httpContextAccessor.HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            if (!string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View();
        }

        public IActionResult Contact()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
            var role = _httpContextAccessor.HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Login", new { area = "" });
            }

            if (!string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View();
        }
    }
}


