using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSProjectAPI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
    public async Task<ActionResult<IEnumerable<object>>> GetAllFeedback()
    {
        try
        {
            var feedbacks = await (from f in _context.Feedbacks
                                   join c in _context.Courses on f.CourseId equals c.CourseId
                                   join u in _context.Users on f.StudentId equals u.UserId
                                   join t in _context.Users on c.TeacherId equals t.UserId
                                   join td in _context.TeacherDetails on t.UserId equals td.UserId
                                   select new
                                   {
                                       // Feedback Fields
                                       FeedbackId = f.FeedbackId,
                                       Comment = f.Comment,
                                       Rating = f.Rating,
                                       FeedbackCreatedAt = f.CreatedAt,
                                       StudentId = f.StudentId,
                                       
                                       // Student Fields
                                       StudentName = u.FullName,
                                       StudentEmail = u.Email,
                                       StudentRole = u.Role,
                                       StudentStatus = u.Status,
                                       StudentCreatedAt = u.CreatedAt,
                                       
                                       // Course Fields
                                       CourseId = c.CourseId,
                                       CourseTitle = c.Title,
                                       CourseDescription = c.Description,
                                       CourseImageUrl = c.ImageUrl,
                                       CourseCreatedAt = c.CreatedAt,
                                       TeacherId = c.TeacherId,
                                       
                                       // Teacher Fields
                                       TeacherName = t.FullName,
                                       TeacherEmail = t.Email,
                                       TeacherRole = t.Role,
                                       TeacherStatus = t.Status,
                                       TeacherCreatedAt = t.CreatedAt,
                                       
                                       // Teacher Detail Fields
                                       Qualification = td.Qualification,
                                       ExperienceYears = td.ExperienceYears
                                   })
                                   .OrderByDescending(x => x.FeedbackCreatedAt)
                                   .ToListAsync();

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

    #region GetAllFeedbackWithFullDetails
    [HttpGet("GetAll")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllFeedbackWithFullDetails()
    {
        try
        {
            var feedbacks = await (from f in _context.Feedbacks
                                   join c in _context.Courses on f.CourseId equals c.CourseId
                                   join u in _context.Users on f.StudentId equals u.UserId
                                   join t in _context.Users on c.TeacherId equals t.UserId
                                   join td in _context.TeacherDetails on t.UserId equals td.UserId
                                   select new
                                   {
                                       // Feedback Information
                                       FeedbackId = f.FeedbackId,
                                       Comment = f.Comment,
                                       Rating = f.Rating,
                                       FeedbackCreatedAt = f.CreatedAt,
                                       StudentId = f.StudentId,
                                       
                                       // Student Information
                                       Student = new
                                       {
                                           UserId = u.UserId,
                                           FullName = u.FullName,
                                           Email = u.Email,
                                           Role = u.Role,
                                           Status = u.Status,
                                           CreatedAt = u.CreatedAt
                                       },
                                       
                                       // Course Information
                                       Course = new
                                       {
                                           CourseId = c.CourseId,
                                           Title = c.Title,
                                           Description = c.Description,
                                           ImageUrl = c.ImageUrl,
                                           CreatedAt = c.CreatedAt,
                                           TeacherId = c.TeacherId
                                       },
                                       
                                       // Teacher Information
                                       Teacher = new
                                       {
                                           UserId = t.UserId,
                                           FullName = t.FullName,
                                           Email = t.Email,
                                           Role = t.Role,
                                           Status = t.Status,
                                           CreatedAt = t.CreatedAt,
                                           Qualification = td.Qualification,
                                           ExperienceYears = td.ExperienceYears
                                       }
                                   })
                                   .OrderByDescending(x => x.FeedbackCreatedAt)
                                   .ToListAsync();

            // Build full image URLs for each feedback
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}".TrimEnd('/');
            
            var result = feedbacks.Select(f => new
            {
                f.FeedbackId,
                f.Comment,
                f.Rating,
                f.FeedbackCreatedAt,
                f.StudentId,
                f.Student,
                Course = new
                {
                    f.Course.CourseId,
                    f.Course.Title,
                    f.Course.Description,
                    f.Course.CreatedAt,
                    f.Course.TeacherId,
                    ImageUrl = string.IsNullOrEmpty(f.Course.ImageUrl) ? null : $"{baseUrl}/{f.Course.ImageUrl}"
                },
                f.Teacher
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error occurred while retrieving detailed feedbacks.",
                error = ex.Message
            });
        }
    }
    #endregion

    #region GetFeedbackByCourse
    [HttpGet("by-course/{courseId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetFeedbackByCourse(int courseId)
    {
        try
        {
            var feedbacks = await (from f in _context.Feedbacks
                                   where f.CourseId == courseId
                                   join c in _context.Courses on f.CourseId equals c.CourseId
                                   join u in _context.Users on f.StudentId equals u.UserId
                                   join t in _context.Users on c.TeacherId equals t.UserId
                                   join td in _context.TeacherDetails on t.UserId equals td.UserId
                                   select new
                                   {
                                       FeedbackId = f.FeedbackId,
                                       Comment = f.Comment,
                                       Rating = f.Rating,
                                       FeedbackCreatedAt = f.CreatedAt,
                                       StudentId = f.StudentId,
                                       Student = new
                                       {
                                           UserId = u.UserId,
                                           FullName = u.FullName,
                                           Email = u.Email
                                       },
                                       Course = new
                                       {
                                           CourseId = c.CourseId,
                                           Title = c.Title
                                       },
                                       Teacher = new
                                       {
                                           UserId = t.UserId,
                                           FullName = t.FullName,
                                           Qualification = td.Qualification,
                                           ExperienceYears = td.ExperienceYears
                                       }
                                   })
                                   .OrderByDescending(x => x.FeedbackCreatedAt)
                                   .ToListAsync();

            return Ok(feedbacks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error occurred while retrieving feedback for course.", error = ex.Message });
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
            feedback.CreatedAt = DateTime.Now;
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Feedback created successfully.", data = feedback });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error occurred while inserting feedback.", error = ex.Message });
        }
    }
    #endregion

    #region UpdateFeedback
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFeedback(int id, [FromBody] Feedback dto)
    {
        if (id != dto.FeedbackId)
            return BadRequest(new { message = "Feedback ID mismatch." });

        var existing = await _context.Feedbacks.FindAsync(id);
        if (existing == null)
            return NotFound(new { message = $"Feedback with ID {id} not found." });

        // ✅ Update only editable fields
        existing.Comment = dto.Comment;
        existing.Rating = dto.Rating;
        existing.CourseId = dto.CourseId;
        existing.StudentId = dto.StudentId;

        // ⚠️ Keep original CreatedAt, optionally add UpdatedAt field if you want
        // existing.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(existing); // ✅ Return updated feedback object
    }
    #endregion

    #region Courses Dropdown
    [HttpGet("course-dropdown")]
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
