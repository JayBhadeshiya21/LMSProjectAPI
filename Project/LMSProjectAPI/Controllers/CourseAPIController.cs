using Microsoft.AspNetCore.Mvc;
using System.Linq;
using LMSProjectAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

[Route("api/[controller]")]
[ApiController]
public class CourseAPIController : ControllerBase
{

    #region Configuration Fields
    private readonly LmsProjectContext _context;
    public CourseAPIController(LmsProjectContext context)
    {
        _context = context;
    }
    #endregion

    #region GetAllCourse
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllCourse()
    {
        try
        {
            var courses = await (from c in _context.Courses
                                 join u in _context.Users
                                     on c.TeacherId equals u.UserId
                                 join td in _context.TeacherDetails
                                     on u.UserId equals td.UserId
                                 select new
                                 {
                                     c.CourseId,
                                     c.Title,
                                     c.Description,
                                     c.CreatedAt,
                                     c.TeacherId,
                                     c.ImageUrl,
                                     TeacherName = u.FullName,            // from User table
                                     Qualification = td.Qualification, // extra from TeacherDetail
                                     ExperienceYears = td.ExperienceYears
                                 })
                                 .ToListAsync();

            return Ok(courses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error occurred while retrieving courses.",
                error = ex.Message
            });
        }
    }


    #endregion

    #region GetCourseById
    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourseById(int id)
    {
        try
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
            return course == null ? NotFound() : Ok(course);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error occurred while retrieving course with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region DeleteCourseById
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourseById(int id)
    {
        try
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while deleting course with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region InsetCourse

    [HttpPost]
    public async Task<IActionResult> InsertCourse([FromForm] CourseDto dto)
    {
        try
        {
            string savedPath = string.Empty;

            // Handle image upload
            if (dto.Image != null && dto.Image.Length > 0)
            {
                savedPath = ImageHelper.SaveImageToFile(dto.Image); // e.g., "uploads/course123.jpg"
                if (string.IsNullOrEmpty(savedPath))
                    return BadRequest("Failed to upload image.");
            }

            // Save course to DB
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                TeacherId = dto.TeacherId,
                CreatedAt = dto.CreatedAt ?? DateTime.Now,
                ImageUrl = savedPath // store only relative path
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Return a full URL for frontend usage
            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
            var imageFullUrl = string.IsNullOrEmpty(course.ImageUrl) ? null : baseUrl + course.ImageUrl;

            return Ok(new
            {
                course.CourseId,
                course.Title,
                course.Description,
                course.TeacherId,
                course.CreatedAt,
                ImageUrl = imageFullUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting course.", error = ex.Message });
        }
    }

    #endregion

    #region UpdateCourse
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromForm] CourseDto dto)
    {
        try
        {
            var existingCourse = await _context.Courses.FindAsync(id);

            if (existingCourse == null)
                return NotFound(new { message = $"Course with ID {id} not found." });

            // Update properties manually without using mapper
            existingCourse.Title = dto.Title;
            existingCourse.Description = dto.Description;
            existingCourse.TeacherId = dto.TeacherId;

            // Only update CreatedAt if it's provided, otherwise keep the original
            if (dto.CreatedAt.HasValue)
            {
                existingCourse.CreatedAt = dto.CreatedAt;
            }

            if (dto.Image != null && dto.Image.Length > 0)
            {
                // Delete old image if path exists
                if (!string.IsNullOrEmpty(existingCourse.ImageUrl))
                {
                    try
                    {
                        ImageHelper.DeleteFile(existingCourse.ImageUrl);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with the update
                        // You might want to add proper logging here
                    }
                }

                // Save new image
                var savedPath = ImageHelper.SaveImageToFile(dto.Image);
                if (string.IsNullOrEmpty(savedPath))
                    return BadRequest("Failed to upload image.");

                existingCourse.ImageUrl = savedPath;
            }

            await _context.SaveChangesAsync();

            // Return a full URL for frontend usage (consistent with insert response)
            var baseUrl = $"{Request.Scheme}://{Request.Host}/";
            var imageFullUrl = string.IsNullOrEmpty(existingCourse.ImageUrl) ? null : baseUrl + existingCourse.ImageUrl;

            return Ok(new
            {
                existingCourse.CourseId,
                existingCourse.Title,
                existingCourse.Description,
                existingCourse.TeacherId,
                existingCourse.CreatedAt,
                ImageUrl = imageFullUrl
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Courses.Any(c => c.CourseId == id))
                return NotFound(new { message = $"Course with ID {id} not found." });
            else
                return BadRequest(new { message = "Concurrency error occurred while updating the course." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while updating course with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region GetAll Course Dropdown
    [HttpGet("dropdown/course")]
    public async Task<ActionResult<IEnumerable<object>>> GetCourseDropdown()
    {
        try
        {
            return await _context.Courses
                .Select(c => new { c.CourseId, c.Title })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while fetching course dropdown.", error = ex.Message });
        }
    }
    #endregion

    #region Teachers Dropdown
    [HttpGet("dropdown")]
    public async Task<ActionResult<IEnumerable<TeacherDropdownDto>>> GetTeachersDropdown()
    {
        try
        {
            var teachers = await _context.TeacherDetails
                .Where(td => td.User != null) // only if User exists
                .Select(td => new TeacherDropdownDto
                {
                    Id = td.UserId,
                    Name = td.User.FullName
                })
                .OrderBy(t => t.Name)
                .ToListAsync();

            return Ok(teachers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while fetching teachers dropdown.", error = ex.Message });
        }
    }
    #endregion

    #region Pagination
    [HttpGet("GetPaged")]
    public async Task<IActionResult> GetPagedCourses(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Courses.AsQueryable();

            // Total records
            var totalRecords = await query.CountAsync();

            // Paginated data
            var courses = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return with pagination info
            return Ok(new
            {
                totalRecords,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                data = courses
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving paged courses", error = ex.Message });
        }
    }

    #endregion
}





