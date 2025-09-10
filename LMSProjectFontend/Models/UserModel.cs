
using System;
using System.ComponentModel.DataAnnotations;

namespace LMSProjectFontend.Models
{
    public class UserModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Enter a Full Name")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email is not valid")]
        [RegularExpression(@"^(?!Demo@gmail\.com$).*", ErrorMessage = "This email is not allowed")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Role length must be between 5 to 10 characters")]
        public string Role { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Status { get; set; }
    }
}
