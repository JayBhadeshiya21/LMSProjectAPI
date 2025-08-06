using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProjectAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class StudentDetailAPIController : ControllerBase
{
    #region Configuration Fields
    private readonly LmsProjectContext _context;
    public StudentDetailAPIController(LmsProjectContext context)
    {
        _context = context;
    }
    #endregion

    #region GetAll
    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<StudentDetail>>> GetAll()
    {
        try
        {
            var students = await _context.StudentDetails.ToListAsync();
            return Ok(students);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while retrieving students.", error = ex.Message });
        }
    }
    #endregion

    #region GetById
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDetail>> GetById(int id)
    {
        try
        {
            var student = await _context.StudentDetails.FindAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error occurred while retrieving student with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region Insert
    [HttpPost]
    public async Task<IActionResult> Insert([FromBody] StudentDetail student)
    {
        try
        {
            _context.StudentDetails.Add(student);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting student.", error = ex.Message });
        }
    }
    #endregion

    #region Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentDetail student)
    {
        if (id != student.UserId)
            return BadRequest();

        _context.Entry(student).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.StudentDetails.Any(s => s.UserId == id))
                return NotFound();
            else
                throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while updating student with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var student = await _context.StudentDetails.FindAsync(id);
            if (student == null) return NotFound();

            _context.StudentDetails.Remove(student);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while deleting student with ID {id}.", error = ex.Message });
        }
    }
    #endregion

}
