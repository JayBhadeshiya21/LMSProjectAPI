//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using LMSProjectAPI;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace LMSProjectAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UserAPIController : ControllerBase
//    {
//        #region Configuration Fields
//        private readonly LmsProjectContext _context;

//        public UserAPIController(LmsProjectContext context)
//        {
//            _context = context;
//        }
//        #endregion

//        #region GetAllUser
//        [HttpGet("All")]
//        public async Task<ActionResult<IEnumerable<User>>> GetAllUser()
//        {
//            try
//            {
//                var users = await _context.Users.ToListAsync();
//                return Ok(users);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, ex.Message);
//            }

//        }
//        #endregion

//        #region GetByUser
//        [HttpGet("{id}")]
//        public async Task<ActionResult<User>> GetUserById(int id)
//        {
//            try
//            {
//                var user = await _context.Users.FindAsync(id);
//                if (user == null)
//                    return NotFound();

//                return Ok(user);
//            }
//            catch(Exception ex)
//            {
//                return StatusCode(500, ex.Message);
//            }

//        }
//        #endregion

//        #region DeleteById
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteUserById(int id)
//        {
//            try
//            {
//                var user = await _context.Users.FindAsync(id);
//                if (user == null)
//                    return NotFound();

//                _context.Users.Remove(user);
//                await _context.SaveChangesAsync();
//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new { message = $"Error deleting user with ID {id}.", error = ex.Message });
//            }

//        }
//        #endregion

//        #region InsertUser
//        [HttpPost]
//        public async Task<IActionResult> InsertUsers([FromBody] User user)
//        {
//            try
//            {
//                _context.Users.Add(user);
//                await _context.SaveChangesAsync();
//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new { message = $"Error occurred while saving user data ", error = ex.Message });

//            }

//        }
//        #endregion

//        #region UpdateUser
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
//        {
//            try
//            {
//                if (id != user.UserId)
//                    return BadRequest();

//                _context.Entry(user).State = EntityState.Modified;

//                await _context.SaveChangesAsync();
//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new { message = $"Error occurred while saving user with ID {user.UserId}", error = ex.Message });
//            }


//        }

//        #endregion
//    }
//}

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

        #region GetAllUsers
        [HttpGet("All")]
        [Authorize]
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
    }
}

