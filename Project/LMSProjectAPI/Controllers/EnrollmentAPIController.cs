using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProjectAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
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
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting enrollment.", error = ex.Message });
        }
    }
    #endregion

    #region UpdateEnrollment
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] Enrollment enrollment)
    {
        if (id != enrollment.EnrollmentId)
            return BadRequest();

        _context.Entry(enrollment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Enrollments.Any(e => e.EnrollmentId == id))
                return NotFound();
            else
                throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while updating enrollment with ID {id}.", error = ex.Message });
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

}
