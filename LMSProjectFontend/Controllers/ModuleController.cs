using System.Net.Http.Headers;
using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LMSProjectFontend.Controllers
{
    public class ModuleController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<ModuleController> _logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public ModuleController(IHttpClientFactory httpClientFactory, ILogger<ModuleController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

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

        #region Get All
        public async Task<IActionResult> Index()
        {
            try
            {
                AttachToken();
                var response = await _client.GetAsync("ModuleAPI/All"); 
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<ModuleModel>>(json) ?? new List<ModuleModel>();
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching modules");
                TempData["Error"] = "Unable to load modules.";
                return View(new List<ModuleModel>());
            }
        }
        #endregion

        #region AddEdit
        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id)
        {
            ViewBag.Courses = new SelectList(await GetCoursesDropdownAsync(), "CourseId", "Title");
            if (id == null || id == 0)
                return View(new ModuleModel());

            try
            {
                AttachToken();
                var response = await _client.GetAsync($"ModuleAPI/{id}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var item = JsonConvert.DeserializeObject<ModuleModel>(json);
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading module");
                TempData["Error"] = "Unable to load module.";
                return RedirectToAction("Index");
            }
        }
       

        [HttpPost]
        public async Task<IActionResult> AddEdit(ModuleModel module)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct validation errors.";
                ViewBag.Courses = new SelectList(await GetCoursesDropdownAsync(), "CourseId", "Title");
                return View("AddEdit", module);
            }

            try
            {
                var jsonData = JsonConvert.SerializeObject(module);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                AttachToken();
                HttpResponseMessage response = module.ModuleId == 0
                    ? await _client.PostAsync("ModuleAPI", content)
                    : await _client.PutAsync($"ModuleAPI/{module.ModuleId}", content);

                response.EnsureSuccessStatusCode();
                TempData["Success"] = module.ModuleId == 0 ? "Module added successfully." : "Module updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving module");
                TempData["Error"] = "Failed to save module.";
                return View("AddEdit", module);
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                AttachToken();
                var response = await _client.DeleteAsync($"ModuleAPI/{id}");
                response.EnsureSuccessStatusCode();
                TempData["Success"] = "Module deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting module");
                TempData["Error"] = "Unable to delete module.";
            }
            return RedirectToAction("Index", "Module");
        }
        #endregion

        #region Dropdown
        private async Task<List<CourseModel>> GetCoursesDropdownAsync()
        {
            try
            {
                var response = await _client.GetAsync("CourseAPI");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<CourseModel>>(json) ?? new List<CourseModel>();
                return list;
            }
            catch
            {
                return new List<CourseModel>();
            }
        }
        #endregion
    }
}


