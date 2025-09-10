using FluentValidation;
using LMSProjectAPI.Models; // make sure you import your User model namespace

namespace LMSProjectAPI.Validation
{
    public class UserValidation : AbstractValidator<User>
    {
        public UserValidation() 
        {
            RuleFor(user => user.FullName)
                .NotEmpty()
                .WithMessage("Enter a Full Name");

            RuleFor(user => user.Email)
                .NotEmpty()
                .EmailAddress()
                .NotEqual("Demo@gmail.com")
                .WithMessage("Email is not valid");

            RuleFor(user => user.Role)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(10)
                .WithMessage("Role length must be between 5 to 10 characters");

            RuleFor(user => user.Password)
                .NotEmpty()
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        }
    }
}
