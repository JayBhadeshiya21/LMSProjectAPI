using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProjectAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EnrollmentAPIController : ControllerBase
{
    private readonly LmsProjectContext _context;

    public EnrollmentAPIController(LmsProjectContext context)
    {
        _context = context;
    }

    #region GetAllEnrollments
    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetAllEnrollments()
    {
        try
        {
            var enrollments = await (from e in _context.Enrollments
                                     join c in _context.Courses on e.CourseId equals c.CourseId
                                     join u in _context.Users on e.StudentId equals u.UserId
                                     select new EnrollmentDto
                                     {
                                         EnrollmentId = e.EnrollmentId,
                                         CourseId = c.CourseId,
                                         CourseName = c.Title,
                                         StudentId = u.UserId,
                                         StudentName = u.FullName,
                                         EnrolledOn = e.EnrolledOn
                                     }).ToListAsync();

            return Ok(enrollments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error occurred while retrieving enrollments.",
                error = ex.Message
            });
        }
    }

    #endregion

    #region GetEnrollmentById
    [HttpGet("{id}")]
    public async Task<ActionResult<Enrollment>> GetEnrollmentById(int id)
    {
        try
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            return Ok(enrollment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error retrieving enrollment with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region InsertEnrollment
    [HttpPost]
    public async Task<IActionResult> InsertEnrollment([FromBody] Enrollment enrollment)
    {
        try
        {
            enrollment.EnrolledOn = DateTime.Now;
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback created successfully.", data = enrollment });
        }
        catch (Exception ex) {
            return BadRequest(new { message = "Error occurred while inserting enrollment.", error = ex.Message });
        }
    }
    #endregion

    #region UpdateEnrollment
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] Enrollment dto)
    {
        if (id != dto.EnrollmentId)
            return BadRequest(new { message = "Enrollment ID mismatch." });

        // Validate
        if (dto.StudentId == 0 || dto.CourseId == 0 || dto.EnrolledOn == default)
            return BadRequest(new { message = "StudentId, CourseId and EnrolledOn are required." });

        try
        {
            var existingEnrollment = await _context.Enrollments.FindAsync(id);
            if (existingEnrollment == null)
                return NotFound(new { message = $"Enrollment with ID {id} not found." });

            // Update allowed fields only
            existingEnrollment.StudentId = dto.StudentId;
            existingEnrollment.CourseId = dto.CourseId;
            existingEnrollment.EnrolledOn = dto.EnrolledOn;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Enrollment updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = $"Error occurred while updating enrollment with ID {id}.",
                error = ex.Message
            });
        }
    }
    #endregion

    #region DeleteEnrollment
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        try
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
                return NotFound();

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while deleting enrollment with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region Courses Dropdown
    [HttpGet("courses-dropdown")]
    public async Task<ActionResult<IEnumerable<CourseDropdownDto>>> GetCoursesDropdown()
    {
        try
        {
            var courses = await _context.Courses
                .Select(c => new CourseDropdownDto
                {
                    Id = c.CourseId,
                    Name = c.Title
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(courses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while fetching courses dropdown.", error = ex.Message });
        }
    }
    #endregion

    #region Students Dropdown
    [HttpGet("student-dropdown")]
    public async Task<ActionResult<IEnumerable<StudentDropdownDto>>> GetStudentsDropdown()
    {
        try
        {
            var students = await _context.Users
                .Where(u => u.Role == "Student") // ✅ filter only students
                .Select(u => new StudentDropdownDto
                {
                    Id = u.UserId,
                    Name = u.FullName
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Ok(students);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while fetching students dropdown.", error = ex.Message });
        }
    }
    #endregion

}

