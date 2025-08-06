using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace LMSProjectAPI.Validation
{
    public class UserValidation : AbstractValidator<User>
    {
        public UserValidation()
        {
            RuleFor(user => user.FullName)
                .NotEmpty()
                .NotNull()
                .WithMessage("Enter a Full Name");

            RuleFor(user => user.Password)
               .Matches("^[a-zA-Z0-9!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]$")
               .Length(8)
               .WithMessage("Enter Strong Password  !");

            RuleFor(user => user.Email)
                .EmailAddress()
                .NotEqual("Demo@gamil.com")
                .WithMessage("Email Is Not Valid");

            RuleFor(user => user.Role)
                .MinimumLength(5)
                .MaximumLength(10)
                .WithMessage("Length Between 5 to 10 ");

            RuleFor(user => user.Password)
                .Length(10)
                .WithMessage("Enter a Password");

        }
    }
}
