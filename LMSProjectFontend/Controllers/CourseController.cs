using System.Text;
using LMSProjectFontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LMSProjectFontend.Controllers
{
    public class CourseController : Controller
    {
        #region Configuration Fields

        private readonly HttpClient _client;
        private readonly ILogger<CourseController> _logger;
        public CourseController(IHttpClientFactory httpClientFactory, ILogger<CourseController> logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("http://localhost:5281/api/");
            _logger = logger;
        }

        #endregion

        #region GetAll Course
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                // 🔹 Get paged courses
                var response = await _client.GetAsync($"CourseAPI/GetPaged?pageNumber={pageNumber}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var pagedCourses = JsonConvert.DeserializeObject<PagedResponse<CourseModel>>(json) ?? new PagedResponse<CourseModel>();

                // 🔹 Get teachers (dropdown API)
                var teacherResponse = await _client.GetAsync("CourseAPI/dropdown");
                teacherResponse.EnsureSuccessStatusCode();
                var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
                var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(teacherJson);

                // 🔹 Map TeacherName into courses
                foreach (var course in pagedCourses.Data)
                {
                    var teacher = teachers.FirstOrDefault(t => t.Id == course.TeacherId);
                    course.TeacherName = teacher?.Name ?? "Unknown Teacher";

                    // ✅ Display API images by default, or new edited images
                    if (!string.IsNullOrEmpty(course.ImageUrl))
                    {
                        // If it's already a full URL, keep it
                        if (course.ImageUrl.StartsWith("http"))
                        {
                            // Keep as is
                        }
                        // If it's a relative path, make it absolute
                        else if (course.ImageUrl.StartsWith("/"))
                        {
                            course.ImageUrl = $"http://localhost:5281{course.ImageUrl}";
                        }
                        // If it's just a filename, construct the full path
                        else
                        {
                            course.ImageUrl = $"http://localhost:5281/uploads/{course.ImageUrl}";
                        }
                    }
                }

                // ✅ Handle newly uploaded images
                var newImagePath = TempData["NewImagePath"]?.ToString();
                var updatedCourseId = TempData["UpdatedCourseId"]?.ToString();

                if (!string.IsNullOrEmpty(newImagePath) && !string.IsNullOrEmpty(updatedCourseId))
                {
                    var courseId = int.Parse(updatedCourseId);
                    var updatedCourse = pagedCourses.Data.FirstOrDefault(c => c.CourseId == courseId);
                    if (updatedCourse != null)
                    {
                        updatedCourse.ImageUrl = newImagePath;
                    }
                }

                return View(pagedCourses); // 🔹 Pass paged courses with images & teacher names
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching courses.");
                TempData["Error"] = "Unable to load courses.";
                return View(new PagedResponse<CourseModel>());
            }
        }



        #endregion

        #region AddAndEdit
        [HttpGet]
        public async Task<IActionResult> AddEdit(int? id)
        {
            // 🔹 Get teachers from API
            var teachers = await GetTeachersDropdownAsync();
            ViewBag.Teachers = new SelectList(teachers, "Id", "Name");

            if (id == null || id == 0)
            {
                return View(new CourseModel()); // Add mode
            }

            try
            {
                var response = await _client.GetAsync($"CourseAPI/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseModel>(json);

                // ✅ Display API images by default, or new edited images
                if (!string.IsNullOrEmpty(course.ImageUrl))
                {
                    // If it's already a full URL, keep it
                    if (course.ImageUrl.StartsWith("http"))
                    {
                        // Keep as is
                    }
                    // If it's a relative path, make it absolute API URL
                    else
                    {
                        course.ImageUrl = $"http://localhost:5281/{course.ImageUrl}";
                    }
                }

                // Pass teacher list again for Edit mode
                ViewBag.Teachers = new SelectList(teachers, "Id", "Name", course.TeacherId);

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading course with ID {id}");
                TempData["Error"] = "Unable to load course for editing.";
                return RedirectToAction("Index");
            }
        }



        [HttpPost]
        public async Task<IActionResult> AddEdit(CourseModel course, IFormFile courseImage)
        {
            try
            {
                // ✅ Validate image file
                if (courseImage != null && courseImage.Length > 0)
                {
                    // Check file size (max 10MB)
                    if (courseImage.Length > 10 * 1024 * 1024)
                    {
                        TempData["Error"] = "Image file size must be less than 10MB.";
                        return await AddEdit(course.CourseId);
                    }

                    // Check file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                    if (!allowedTypes.Contains(courseImage.ContentType.ToLower()))
                    {
                        TempData["Error"] = "Only JPEG, PNG, and GIF images are allowed.";
                        return await AddEdit(course.CourseId);
                    }
                }

                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(new StringContent(course.CourseId.ToString()), "CourseId");
                    formData.Add(new StringContent(course.Title ?? ""), "Title");
                    formData.Add(new StringContent(course.Description ?? ""), "Description");
                    formData.Add(new StringContent(course.TeacherId.ToString()), "TeacherId");
                    formData.Add(new StringContent(course.CreatedAt.ToString("o")), "CreatedAt");

                    // ✅ Add image file to form data
                    if (courseImage != null && courseImage.Length > 0)
                    {
                        var streamContent = new StreamContent(courseImage.OpenReadStream());
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(courseImage.ContentType);
                        formData.Add(streamContent, "courseImage", courseImage.FileName);
                    }

                    HttpResponseMessage response;
                    if (course.CourseId == 0)
                    {
                        response = await _client.PostAsync("CourseAPI", formData);
                    }
                    else
                    {
                        response = await _client.PutAsync($"CourseAPI/{course.CourseId}", formData);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        // ✅ Save image locally for immediate display
                        if (courseImage != null && courseImage.Length > 0)
                        {
                            var savedFileName = await SaveImageLocally(courseImage, course.CourseId);
                            if (!string.IsNullOrEmpty(savedFileName))
                            {
                                // Store the new image path in TempData for immediate display
                                TempData["NewImagePath"] = $"/uploads/{savedFileName}";
                                TempData["UpdatedCourseId"] = course.CourseId.ToString();
                            }
                        }

                        TempData["Success"] = course.CourseId == 0 ? "Course created successfully!" : "Course updated successfully!";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                        TempData["Error"] = $"Error occurred while saving course. Status: {response.StatusCode}";
                        return await AddEdit(course.CourseId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddEdit POST method");
                TempData["Error"] = "An unexpected error occurred while saving the course.";
                return await AddEdit(course.CourseId);
            }
        }


        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"CourseAPI/{id}");
                response.EnsureSuccessStatusCode();

                TempData["Success"] = "Course deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with ID {id}.");
                TempData["Error"] = "Unable to delete Course.";
            }
            return RedirectToAction("Index", "Course");
        }
        #endregion

        #region Image Helper Methods
        private async Task<string> SaveImageLocally(IFormFile imageFile, int courseId)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return string.Empty;

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName);
                var fileName = $"course_{courseId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                _logger.LogInformation($"Image saved locally: {fileName}");
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image locally");
                return string.Empty;
            }
        }
        #endregion

        #region Image Mutipart Data
        public MultipartFormDataContent ConvertToMultipartFormData(CourseModel course)
        {
            var formData = new MultipartFormDataContent();

            // Add basic properties
            formData.Add(new StringContent(course.CourseId.ToString()), "CourseId");
            formData.Add(new StringContent(course.Title ?? ""), "Title");
            formData.Add(new StringContent(course.Description ?? ""), "Description");
            formData.Add(new StringContent(course.TeacherId?.ToString() ?? ""), "TeacherId");
            formData.Add(new StringContent(course.CreatedAt.ToString("o")), "CreatedAt");
            formData.Add(new StringContent(course.ImageUrl ?? ""), "imageUrl");

            // Add uploaded image file if available
            if (course.CourseImage != null && course.CourseImage.Length > 0)
            {
                var streamContent = new StreamContent(course.CourseImage.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(course.CourseImage.ContentType);

                formData.Add(streamContent, "CourseImage", course.CourseImage.FileName);
            }

            return formData;
        }

        #endregion

        #region Teacher Dropdown
        public async Task<List<TeacherDropdownDto>> GetTeachersDropdownAsync()
        {
            try
            {
                var response = await _client.GetAsync("CourseAPI/dropdown"); // 🔹 your API endpoint
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

        public async Task<IActionResult> Indexed(int pageNumber = 1, int pageSize = 5)
        {
            var response = await _client.GetAsync($"CourseAPI/GetAll?pageNumber={pageNumber}&pageSize={pageSize}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to fetch courses";
                return View(new PagedResponse<CourseModel>());
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var coursesPaged = JsonConvert.DeserializeObject<PagedResponse<CourseModel>>(jsonString);

            return View(coursesPaged); // ✅ send PagedResponse not List
        }

        #region Search
        [HttpGet]
        public async Task<IActionResult> Search(string keyword = "", int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Try API search first
                var searchUrl = $"CourseAPI/Search?keyword={Uri.EscapeDataString(keyword)}&pageNumber={pageNumber}&pageSize={pageSize}";
                
                var response = await _client.GetAsync(searchUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var searchResults = JsonConvert.DeserializeObject<PagedResponse<CourseModel>>(json) ?? new PagedResponse<CourseModel>();

                    // Get teachers for mapping TeacherName
                    var teacherResponse = await _client.GetAsync("CourseAPI/dropdown");
                    if (teacherResponse.IsSuccessStatusCode)
                    {
                        var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
                        var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(teacherJson);

                        // Map TeacherName into courses
                        foreach (var course in searchResults.Data)
                        {
                            var teacher = teachers?.FirstOrDefault(t => t.Id == course.TeacherId);
                            course.TeacherName = teacher?.Name ?? "Unknown Teacher";

                            // Fix image URLs
                            if (!string.IsNullOrEmpty(course.ImageUrl))
                            {
                                if (!course.ImageUrl.StartsWith("http"))
                                {
                                    if (course.ImageUrl.StartsWith("/"))
                                    {
                                        course.ImageUrl = $"http://localhost:5281{course.ImageUrl}";
                                    }
                                    else
                                    {
                                        course.ImageUrl = $"http://localhost:5281/uploads/{course.ImageUrl}";
                                    }
                                }
                            }
                        }
                    }

                    return Json(searchResults);
                }
                else
                {
                    // Fallback: Get all courses and search locally
                    return await LocalSearch(keyword, pageNumber, pageSize);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching courses via API. Falling back to local search.");
                // Fallback to local search
                return await LocalSearch(keyword, pageNumber, pageSize);
            }
        }

        private async Task<IActionResult> LocalSearch(string keyword, int pageNumber, int pageSize)
        {
            try
            {
                // Get all courses
                var response = await _client.GetAsync("CourseAPI/GetAll");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var allCourses = JsonConvert.DeserializeObject<List<CourseModel>>(json) ?? new List<CourseModel>();

                // Get teachers for mapping
                var teacherResponse = await _client.GetAsync("CourseAPI/dropdown");
                if (teacherResponse.IsSuccessStatusCode)
                {
                    var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
                    var teachers = JsonConvert.DeserializeObject<List<TeacherDropdownDto>>(teacherJson);

                    // Map TeacherName and fix image URLs
                    foreach (var course in allCourses)
                    {
                        var teacher = teachers?.FirstOrDefault(t => t.Id == course.TeacherId);
                        course.TeacherName = teacher?.Name ?? "Unknown Teacher";

                        if (!string.IsNullOrEmpty(course.ImageUrl))
                        {
                            if (!course.ImageUrl.StartsWith("http"))
                            {
                                if (course.ImageUrl.StartsWith("/"))
                                {
                                    course.ImageUrl = $"http://localhost:5281{course.ImageUrl}";
                                }
                                else
                                {
                                    course.ImageUrl = $"http://localhost:5281/uploads/{course.ImageUrl}";
                                }
                            }
                        }
                    }
                }

                // Filter courses based on keyword
                var filteredCourses = allCourses;
                if (!string.IsNullOrEmpty(keyword))
                {
                    var searchTerm = keyword.ToLower();
                    filteredCourses = allCourses.Where(c => 
                        (c.Title?.ToLower().Contains(searchTerm) ?? false) ||
                        (c.Description?.ToLower().Contains(searchTerm) ?? false) ||
                        (c.TeacherName?.ToLower().Contains(searchTerm) ?? false)
                    ).ToList();
                }

                // Apply pagination
                var totalRecords = filteredCourses.Count;
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                var pagedCourses = filteredCourses
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PagedResponse<CourseModel>
                {
                    Data = pagedCourses,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in local search fallback");
                return Json(new PagedResponse<CourseModel>());
            }
        }
        #endregion


    }
}


