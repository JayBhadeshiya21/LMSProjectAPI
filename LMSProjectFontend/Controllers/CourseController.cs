//using System.Text;
//using LMSProjectFontend.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ModelBinding;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Microsoft.AspNetCore.Http;

//namespace LMSProjectFontend.Controllers
//{
//    public class CourseController : Controller
//    {
//        #region Configuration Fields

//        private readonly HttpClient _client;
//        private readonly ILogger<CourseController> _logger;
//        public CourseController(IHttpClientFactory httpClientFactory, ILogger<CourseController> logger)
//        {
//            _client = httpClientFactory.CreateClient();
//            _client.BaseAddress = new Uri("http://localhost:5281/api/");
//            _logger = logger;
//        }

//        #endregion

//        #region GetAll Course
//        public async Task<IActionResult> Index()
//        {
//            try
//            {
//                var response = await _client.GetAsync("CourseAPI");
//                if (response.IsSuccessStatusCode)
//                {
//                    var json = await response.Content.ReadAsStringAsync();
//                    var courses = JsonConvert.DeserializeObject<List<CourseModel>>(json) ?? new List<CourseModel>();

//                    // Get teachers for mapping TeacherName
//                    var teacherResponse = await _client.GetAsync("CourseAPI/dropdown");
//                    if (teacherResponse.IsSuccessStatusCode)
//                    {
//                        var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
//                        var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(teacherJson);

//                        // Map TeacherName into courses
//                        foreach (var course in courses)
//                        {
//                            var teacher = teachers?.FirstOrDefault(t => t.Id == course.TeacherId);
//                            course.TeacherName = teacher?.Name ?? "Unknown Teacher";
//                        }
//                    }

//                    return View(courses);
//                }
//                else
//                {
//                    _logger.LogError("Failed to fetch courses. Status: {StatusCode}", response.StatusCode);
//                    return View(new List<CourseModel>());
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while fetching courses");
//                return View(new List<CourseModel>());
//            }
//        }
//        #endregion

//        #region AddAndEdit
//        public async Task<IActionResult> AddEdit(int id = 0)
//        {
//            try
//            {
//                // Always populate teachers dropdown
//                var teacherResponse = await _client.GetAsync("CourseAPI/dropdown");
//                if (teacherResponse.IsSuccessStatusCode)
//                {
//                    var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
//                    var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(teacherJson) ?? new List<TeacherDropdownDto>();
//                    ViewBag.Teachers = new SelectList(teachers, "Id", "Name");
//                }

//                if (id == 0)
//                {
//                    return View(new CourseModel());
//                }

//                var response = await _client.GetAsync($"CourseAPI/GetById/{id}");
//                if (response.IsSuccessStatusCode)
//                {
//                    var json = await response.Content.ReadAsStringAsync();
//                    var course = JsonConvert.DeserializeObject<CourseModel>(json);
//                    if (course != null)
//                    {
//                        return View(course);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while fetching course for edit");
//            }

//            return RedirectToAction(nameof(Index));
//        }



//        [HttpPost]
//        public async Task<IActionResult> AddEdit(CourseModel course, IFormFile? imageFile)
//        {
//            try
//            {
//                // Prefer multipart to let API handle image storage and return final path
//                using var formData = new MultipartFormDataContent();

//                formData.Add(new StringContent(course.CourseId.ToString()), "CourseId");
//                formData.Add(new StringContent(course.Title ?? string.Empty), "Title");
//                formData.Add(new StringContent(course.Description ?? string.Empty), "Description");
//                formData.Add(new StringContent(course.TeacherId?.ToString() ?? string.Empty), "TeacherId");
//                formData.Add(new StringContent(course.CreatedAt.ToString("o")), "CreatedAt");

//                // Pass through existing ImageUrl so API can keep it if no new file uploaded
//                formData.Add(new StringContent(course.ImageUrl ?? string.Empty), "ImageUrl");

//                // Prefer bound CourseImage; fallback to imageFile name if provided
//                var uploadFile = course.CourseImage ?? imageFile;
//                if (uploadFile != null && uploadFile.Length > 0)
//                {
//                    var streamContent = new StreamContent(uploadFile.OpenReadStream());
//                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(uploadFile.ContentType);
//                    // Field name aligned with existing helper usage
//                    formData.Add(streamContent, "CourseImage", uploadFile.FileName);
//                }

//                HttpResponseMessage response;
//                if (course.CourseId == 0)
//                {
//                    response = await _client.PostAsync("CourseAPI/Create", formData);
//                }
//                else
//                {
//                    response = await _client.PutAsync($"CourseAPI/Update/{course.CourseId}", formData);
//                }

//                if (response.IsSuccessStatusCode)
//                {
//                    // Optionally read back the updated course (including API-resolved ImageUrl)
//                    var respJson = await response.Content.ReadAsStringAsync();
//                    var updated = JsonConvert.DeserializeObject<CourseModel>(respJson);
//                    if (updated != null)
//                    {
//                        TempData["Success"] = "Course saved successfully.";
//                    }
//                    return RedirectToAction(nameof(Index));
//                }
//                else
//                {
//                    _logger.LogError("Failed to save course. Status: {StatusCode}", response.StatusCode);
//                    ModelState.AddModelError("", "Failed to save course. Please try again.");
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while saving course");
//                ModelState.AddModelError("", "An error occurred. Please try again.");
//            }

//            // If we get here, something went wrong, so reload the teachers for the dropdown
//            try
//            {
//                var teacherResponse = await _client.GetAsync("CourseAPI/dropdown");
//                if (teacherResponse.IsSuccessStatusCode)
//                {
//                    var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
//                    var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(teacherJson) ?? new List<TeacherDropdownDto>();
//                    ViewBag.Teachers = new SelectList(teachers, "Id", "Name");
//                }
//            }
//            catch
//            {
//                // Ignore errors when reloading teachers
//            }

//            return View(course);
//        }


//        #endregion

//        #region Delete
//        [HttpPost]
//        public async Task<IActionResult> Delete(int id)
//        {
//            try
//            {
//                var response = await _client.DeleteAsync($"CourseAPI/Delete/{id}");
//                if (response.IsSuccessStatusCode)
//                {
//                    return Json(new { success = true });
//                }
//                else
//                {
//                    _logger.LogError("Failed to delete course. Status: {StatusCode}", response.StatusCode);
//                    return Json(new { success = false, message = "Failed to delete course" });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while deleting course");
//                return Json(new { success = false, message = "An error occurred while deleting the course" });
//            }
//        }
//        #endregion

//        #region Image Mutipart Data
//        public MultipartFormDataContent ConvertToMultipartFormData(CourseModel course)
//        {
//            var formData = new MultipartFormDataContent();

//            // Add basic properties
//            formData.Add(new StringContent(course.CourseId.ToString()), "CourseId");
//            formData.Add(new StringContent(course.Title ?? ""), "Title");
//            formData.Add(new StringContent(course.Description ?? ""), "Description");
//            formData.Add(new StringContent(course.TeacherId?.ToString() ?? ""), "TeacherId");
//            formData.Add(new StringContent(course.CreatedAt.ToString("o")), "CreatedAt");
//            formData.Add(new StringContent(course.ImageUrl ?? ""), "imageUrl");

//            // Add uploaded image file if available
//            if (course.ImagePath != null && course.CourseImage.Length > 0)
//            {
//                var streamContent = new StreamContent(course.CourseImage.OpenReadStream());
//                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(course.CourseImage.ContentType);

//                formData.Add(streamContent, "CourseImage", course.CourseImage.FileName);
//            }

//            return formData;
//        }

//        #endregion
//    }
//}


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
    public class CourseController : Controller
    {
        #region Fields & Constructor

        private readonly HttpClient _client;
        private readonly ILogger<CourseController> _logger;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public CourseController(IHttpClientFactory httpClientFactory, ILogger<CourseController> logger, IHttpContextAccessor httpContextAccessor)
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

        #region Index
        public async Task<IActionResult> Index()
        {
            try
            {
                AttachToken();
                var response = await _client.GetAsync("CourseAPI");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch courses. Status: {StatusCode}", response.StatusCode);
                    return View(new List<CourseModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var courses = JsonConvert.DeserializeObject<List<CourseModel>>(json) ?? new List<CourseModel>();

                // Map teacher names
                var teachers = await GetTeachersDropdownAsync();
                foreach (var course in courses)
                {
                    var teacher = teachers.FirstOrDefault(t => t.Id == course.TeacherId);
                    course.TeacherName = teacher?.Name ?? "Unknown Teacher";
                }

                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching courses");
                return View(new List<CourseModel>());
            }
        }
        #endregion

        #region Add/Edit
        [HttpGet]
        public async Task<IActionResult> AddEdit(int id = 0)
        {
            try
            {
                ViewBag.Teachers = new SelectList(await GetTeachersDropdownAsync(), "Id", "Name");

                if (id == 0)
                    return View(new CourseModel());

                AttachToken();
                var response = await _client.GetAsync($"CourseAPI/GetById/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var course = JsonConvert.DeserializeObject<CourseModel>(json);
                    if (course != null)
                        return View(course);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching course for edit");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(CourseModel course)
        {
            try
            {
                var formData = ConvertToMultipartFormData(course);

                HttpResponseMessage response;
                if (course.CourseId == 0)
                {
                    // Create new course
                    AttachToken();
                    var request = new HttpRequestMessage(HttpMethod.Post, "CourseAPI");
                    request.Content = formData;
                    response = await _client.SendAsync(request);
                }
                else
                {
                    // Update existing course
                    AttachToken();
                    var request = new HttpRequestMessage(HttpMethod.Put, $"CourseAPI/{course.CourseId}");
                    request.Content = formData;
                    response = await _client.SendAsync(request);
                }

                if (response.IsSuccessStatusCode)
                {
                    if (course.CourseId > 0)
                    {
                        TempData["SuccessMessage"] = "Course updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Course created successfully.";
                        return RedirectToAction(nameof(AddEdit));
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Error occurred while saving course.";
                    return View("AddEdit", course);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving course");
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return View("AddEdit", course);
            }
        }

        #endregion

        #region Helper
        private MultipartFormDataContent ConvertToMultipartFormData(CourseModel course)
        {
            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(course.CourseId.ToString() ?? ""), "CourseId");
            formData.Add(new StringContent(course.Title ?? ""), "Title");
            formData.Add(new StringContent(course.Description ?? ""), "Description");
            formData.Add(new StringContent(course.TeacherId?.ToString() ?? ""), "TeacherId");
            formData.Add(new StringContent(course.CreatedAt.ToString("o")), "CreatedAt");
            formData.Add(new StringContent(course.ImageUrl ?? ""), "ImageUrl");

            if (course.CourseImage != null && course.CourseImage.Length > 0)
            {
                var streamContent = new StreamContent(course.CourseImage.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(course.CourseImage.ContentType);

                formData.Add(streamContent, "CourseImage", course.CourseImage.FileName);
            }

            return formData;
        }
        #endregion

        #region Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                AttachToken();
                var response = await _client.DeleteAsync($"CourseAPI/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Course deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete course.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while deleting the course.";
            }

            return RedirectToAction("Index", "Course");
        }
        #endregion

        #region Teacher Dropdown
        public async Task<List<TeacherDropdownDto>> GetTeachersDropdownAsync()
        {
            try
            {
                AttachToken();
                var response = await _client.GetAsync("CourseAPI/dropdown");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(json);

                return teachers ?? new List<TeacherDropdownDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher dropdown.");
                return new List<TeacherDropdownDto>();
            }
        }
        #endregion

        #region Search
        [HttpGet]
        public async Task<IActionResult> Search(string keyword = "", int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                AttachToken();
                var searchUrl = $"CourseAPI/Search?keyword={Uri.EscapeDataString(keyword)}&pageNumber={pageNumber}&pageSize={pageSize}";
                var response = await _client.GetAsync(searchUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var searchResults = JsonConvert.DeserializeObject<PagedResponse<CourseModel>>(json) ?? new PagedResponse<CourseModel>();

                    var teachers = await GetTeachersDropdownAsync();
                    foreach (var course in searchResults.Data)
                    {
                        var teacher = teachers.FirstOrDefault(t => t.Id == course.TeacherId);
                        course.TeacherName = teacher?.Name ?? "Unknown Teacher";
                    }

                    return Json(searchResults);
                }

                return await LocalSearch(keyword, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching courses via API. Falling back to local search.");
                return await LocalSearch(keyword, pageNumber, pageSize);
            }
        }

        private async Task<IActionResult> LocalSearch(string keyword, int pageNumber, int pageSize)
        {
            try
            {
                AttachToken();
                var response = await _client.GetAsync("CourseAPI/GetAll");
                if (!response.IsSuccessStatusCode)
                    return Json(new PagedResponse<CourseModel>());

                var json = await response.Content.ReadAsStringAsync();
                var allCourses = JsonConvert.DeserializeObject<List<CourseModel>>(json) ?? new List<CourseModel>();

                var teachers = await GetTeachersDropdownAsync();
                foreach (var course in allCourses)
                {
                    var teacher = teachers.FirstOrDefault(t => t.Id == course.TeacherId);
                    course.TeacherName = teacher?.Name ?? "Unknown Teacher";
                }

                var filteredCourses = allCourses.Where(c =>
                    string.IsNullOrEmpty(keyword) ||
                    c.Title?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                    c.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true ||
                    c.TeacherName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true
                ).ToList();

                var totalCount = filteredCourses.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var pagedCourses = filteredCourses
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PagedResponse<CourseModel>
                {
                    Data = pagedCourses,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during local search");
                return Json(new PagedResponse<CourseModel>());
            }
        }
        #endregion
    }
}







