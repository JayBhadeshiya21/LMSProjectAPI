using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProjectAPI;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class FeedbackAPIController : ControllerBase
{
    #region Configuration Fields
    private readonly LmsProjectContext _context;
    public FeedbackAPIController(LmsProjectContext context)
    {
        _context = context;
    }
    #endregion

    #region GetAllFeedback
    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetAllFeedback()
    {
        try
        {
            var feedbacks = await (from f in _context.Feedbacks
                                   join c in _context.Courses on f.CourseId equals c.CourseId
                                   join u in _context.Users on f.StudentId equals u.UserId
                                   select new FeedbackDto
                                   {
                                       FeedbackId = f.FeedbackId,
                                       Comments = f.Comment,
                                       Rating = f.Rating,
                                       CourseId = c.CourseId,
                                       CourseName = c.Title,
                                       StudentId = u.UserId,
                                       StudentName = u.FullName, 
                                       CreatedAt = f.CreatedAt
                                   }).ToListAsync();

            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error occurred while retrieving feedbacks.",
                error = ex.Message
            });
        }
    }

    #endregion

    #region GetFeedbackById
    [HttpGet("{id}")]
    public async Task<ActionResult<Feedback>> GetFeedbackById(int id)
    {
        try
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            return Ok(feedback);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error occurred while retrieving feedback with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region DeleteFeedbackById
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFeedbackById(int id)
    {
        try
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while deleting feedback with ID {id}.", error = ex.Message });
        }
    }
    #endregion

    #region InsertFeedback
    [HttpPost]
    public async Task<IActionResult> InsertFeedback([FromBody] Feedback feedback)
    {
        try
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting feedback.", error = ex.Message });
        }
    }
    #endregion

    #region UpdateFeedback
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFeedback(int id, [FromBody] Feedback feedback)
    {
        if (id != feedback.FeedbackId)
            return BadRequest();

        _context.Entry(feedback).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Feedbacks.Any(f => f.FeedbackId == id))
                return NotFound();
            else
                throw;
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Error occurred while updating feedback with ID {id}.", error = ex.Message });
        }
    }
    #endregion

}
