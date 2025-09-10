using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMSProjectAPI;
using LMSProjectAPI.Models;

namespace LMSProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAPIController : ControllerBase
    {
        #region Configuration Fields
        private readonly LmsProjectContext _context;
        private readonly IConfiguration _configuration;

        public UserAPIController(LmsProjectContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        #endregion
      
        #region JWT Token Generator
        private string GenerateJwtToken(UserLogin user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("Password", user.Password ?? ""), // ⚠️ avoid storing password in JWT in real-world apps
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var expiryMinutes = Convert.ToDouble(jwtSettings["TokenExpiryMinutes"]);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region Register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(new { message = "Invalid user payload." });
                }

                // Enforce Student role for public registration
                user.Role = "Student";

                // Prevent duplicate emails
                var exists = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (exists)
                {
                    return Conflict(new { message = "Email already registered." });
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Student registered successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error occurred while registering student.", error = ex.Message });
            }
        }
        #endregion

        #region Login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin loginUser)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == loginUser.Email &&
                    u.Password == loginUser.Password &&
                    u.Role == loginUser.Role);

            if (user == null)   
                return Unauthorized(new { message = "Invalid email, password, or role" });

            var token = GenerateJwtToken(loginUser);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Email,
                    user.Password,
                    user.Role
                }
            });
        }
        #endregion

        #region Me
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            try
            {
                var emailClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(emailClaim))
                {
                    return Unauthorized(new { message = "Email claim missing from token." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                if (user == null)
                {
                    return NotFound(new { message = "User not found for current token." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error resolving current user.", error = ex.Message });
            }
        }
        #endregion

        #region GetAllUsers
        [HttpGet("All")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving users.", error = ex.Message });
            }
        }
        #endregion

        #region GetUserById
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = $"User with ID {id} not found." });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user.", error = ex.Message });
            }
        }
        #endregion

        #region InsertUser
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> InsertUser([FromBody] User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error occurred while saving user.", error = ex.Message });
            }
        }
        #endregion

        #region UpdateUser
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            try
            {
                if (id != user.UserId)
                    return BadRequest(new { message = "User ID mismatch." });

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { message = $"User with ID {id} updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating user with ID {id}.", error = ex.Message });
            }
        }
        #endregion
        
        #region DeleteUserById
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = $"User with ID {id} not found." });

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = $"User with ID {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error deleting user with ID {id}.", error = ex.Message });
            }
        }
        #endregion

        #region Search User

        [HttpGet("Search")]
        [AllowAnonymous]
        public IActionResult Search(string? keyword)
        {
            var users = _context.Users
                .Where(u => string.IsNullOrEmpty(keyword) ||
                            u.FullName.Contains(keyword) ||
                            u.Email.Contains(keyword) ||
                            u.Role.Contains(keyword))
                .ToList();

            return Ok(users);
        }

        #endregion

        #region Summary
        [HttpGet("Summary")]
        public IActionResult GetDashboardSummary()
        {
            var summary = new DashboardSummaryDto
            {
                CourseCount = _context.Courses.Count(),
                StudentCount = _context.Users.Count(u => u.Role == "Student"),
                TeacherCount = _context.Users.Count(u => u.Role == "Teacher"),
                EnrollmentCount = _context.Enrollments.Count(),
                FeedbackCount = _context.Feedbacks.Count()
            };

            return Ok(summary);
        }
        #endregion
    }
}

