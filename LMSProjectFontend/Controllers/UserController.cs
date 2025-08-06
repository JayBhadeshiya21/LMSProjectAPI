using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LMSProjectFontend.Controllers
{
    public class UserController : Controller
    {

        #region Configuration Fields
       
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserController> _logger;
        public UserController(IHttpClientFactory httpClientFactory, ILogger<UserController> logger, IHttpContextAccessor httpContextAccessor) 
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Add JWT Token 
        // ✅ Adds JWT token from session to request headers
        private void AddJwtToken()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
        #endregion

        #region GetById

        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new UserModel()); // Add mode
            }

            try
            {
                var response = await _client.GetAsync($"UserAPI/{id}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(json);

                return View(user); // Edit mode with data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading user with ID {id}");
                TempData["Error"] = "Unable to load user for editing.";
                return RedirectToAction("Index");
            }
        }
        #endregion

        #region Get All User
        public async Task<IActionResult> Index()
        {
            try
            {
                AddJwtToken();
                var response = await _client.GetAsync("UserAPI/All");

                // User is unauthorized
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Login");
                }
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<UserModel>>(json);
                return View(list);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching users.");
                TempData["Error"] = "Unable to load users.";
                return View(new List<UserModel>());
            }
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"UserAPI/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with ID {id}.");
                TempData["Error"] = "Unable to delete user.";
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region AddAndEdit

        [HttpPost]
        public async Task<IActionResult> AddEdit(UserModel user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the validation errors.";
                return View(user.UserId == 0 ? "AddEdit" : "AddEdit", user);
            }

            try
            {
                var jsonData = JsonConvert.SerializeObject(user);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                if (user.UserId == 0)
                {
                    user.UserId = 0;
                    user.CreatedAt = DateTime.Now;
                    response = await _client.PostAsync("UserAPI/", content);
                }
                else
                {
                    response = await _client.PutAsync($"UserAPI/{user.UserId}", content);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving user.");
                TempData["Error"] = "Failed to save user. Please try again.";
                return View("AddEdit", user);
            }

            return RedirectToAction("Index");
        }

        #endregion
    }
}
