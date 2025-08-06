using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LMSProjectFontend;
using Microsoft.AspNetCore.Http; // ✅ Needed for Session

namespace LMSProjectFontend.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;


        // ✅ Inject IHttpContextAccessor
        public LoginController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password, string Role)
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

            _httpContextAccessor.HttpContext.Session.SetString("JWTToken", token);
            _httpContextAccessor.HttpContext.Session.SetString("UserRole", Role);

            return RedirectToAction("Index", "Home");
        }

    }
}
