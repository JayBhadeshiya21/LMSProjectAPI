using System.Diagnostics;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LMSProjectFontend.Controllers
{
    public class HomeController : Controller
    {
        #region Configuration Fields
        private readonly HttpClient _client;
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");

            if (string.IsNullOrEmpty(token))
            {
                // Not authenticated, redirect to Login page
                return RedirectToAction("Login", "Login");
            }

            try
            {
                // 🔑 Attach JWT token to request
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Fetch dashboard summary data
                var summaryResponse = await _client.GetAsync("UserAPI/Summary");
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
                _logger.LogError(ex, "Error fetching dashboard summary");

                // Fallback on error
                ViewBag.DashboardSummary = new DashboardSummaryModel();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
