using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProjectAPI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TeacherDetailAPIController : ControllerBase
{
    #region Configuration Fields
    private readonly LmsProjectContext _context;
    public TeacherDetailAPIController(LmsProjectContext context)
    {
        _context = context;
    }
    #endregion

    #region GetAll
    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<TeacherDetail>>> GetAll()
    {
        try
        {
            var teachers = await _context.TeacherDetails.ToListAsync();
            return Ok(teachers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while retrieving teachers.", error = ex.Message });
        }
    }
    #endregion

    #region GetById
    [HttpGet("{id}")]
    public async Task<ActionResult<TeacherDetail>> GetById(int id)
    {
        try
        {
            var teacher = await _context.TeacherDetails.FindAsync(id);
            if (teacher == null) return NotFound();
            return Ok(teacher);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error occurred while retrieving teacher with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region Insert
    [HttpPost]
    public async Task<IActionResult> Insert([FromBody] TeacherDetail teacher)
    {
        try
        {
            _context.TeacherDetails.Add(teacher);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting teacher.", error = ex.Message });
        }
    }
    #endregion

    #region Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TeacherDetail teacher)
    {
        if (id != teacher.UserId)
            return BadRequest();

        _context.Entry(teacher).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.TeacherDetails.Any(t => t.UserId == id))
                return NotFound();
            else
                throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while updating teacher with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var teacher = await _context.TeacherDetails.FindAsync(id);
            if (teacher == null) return NotFound();

            _context.TeacherDetails.Remove(teacher);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while deleting teacher with ID {id}.", error = ex.Message });
        }
    }
    #endregion

}
