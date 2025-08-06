using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace LMSProjectAPI.Validation
{
    public class EnrollmentValidation : AbstractValidator<Enrollment>
    {
        public EnrollmentValidation()
        {
            RuleFor(Enrollment => Enrollment.StudentId)
                .NotEmpty()
                .WithMessage("Enter Student Id !");

            RuleFor(Enrollment => Enrollment.CourseId)
               .NotEmpty()
               .WithMessage("Enter Course Id !");
        }
    }
}
