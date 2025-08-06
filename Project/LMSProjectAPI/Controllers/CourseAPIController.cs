using Microsoft.AspNetCore.Mvc;
using System.Linq;
using LMSProjectAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

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
    public async Task<ActionResult<IEnumerable<Course>>> GetAllCourse()
    {
        try
        {
            return await _context.Courses.Order().ToListAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while retrieving courses.", error = ex.Message });
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

    [HttpPost]
    public async Task<IActionResult> InsertCourse([FromForm] Course model)
    {
        try
        {
            _context.Courses.Add(model);
            await _context.SaveChangesAsync();
            return Ok(model);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting course.", error = ex.Message });
        }
    }

    #region UpdateCourse
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course course)
    {
        if (id != course.CourseId)
            return BadRequest();

        _context.Entry(course).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Courses.Any(c => c.CourseId == id))
                return NotFound();
            else
                throw;
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

}





