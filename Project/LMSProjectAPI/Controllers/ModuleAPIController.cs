using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProjectAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ModuleAPIController : ControllerBase
{
    #region Configuration Fields
    private readonly LmsProjectContext _context;
    public ModuleAPIController(LmsProjectContext context)
    {
        _context = context;
    }
    #endregion

    #region GetAllModules
    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<Module>>> GetAllModules()
    {
        try
        {
            var modules = await _context.Modules.ToListAsync();
            return Ok(modules);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while retrieving modules.", error = ex.Message });
        }
    }
    #endregion

    #region GetModuleById
    [HttpGet("{id}")]
    public async Task<ActionResult<Module>> GetModuleById(int id)
    {
        try
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
                return NotFound();

            return Ok(module);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error occurred while retrieving module with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region DeleteModuleById
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteModuleById(int id)
    {
        try
        {
            var module = await _context.Modules.FindAsync(id);
            if (module == null)
                return NotFound();

            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while deleting module with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region InsertModule
    [HttpPost]
    public async Task<IActionResult> InsertModule([FromBody] Module module)
    {
        try
        {
            _context.Modules.Add(module);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting module.", error = ex.Message });
        }
    }
    #endregion

    #region UpdateModule
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateModule(int id, [FromBody] Module module)
    {
        if (id != module.ModuleId)
            return BadRequest();

        _context.Entry(module).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Modules.Any(m => m.ModuleId == id))
                return NotFound();
            else
                throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while updating module with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region GetAll Module Dropdown
    [HttpGet("dropdown/module")]
    public async Task<ActionResult<IEnumerable<object>>> GetModuleDropdown()
    {
        try
        {
            return await _context.Modules
                .Select(c => new { c.ModuleId, c.Title })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while fetching module dropdown.", error = ex.Message });
        }
    }
    #endregion

}
