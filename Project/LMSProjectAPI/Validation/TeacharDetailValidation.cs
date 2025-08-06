using FluentValidation;

namespace LMSProjectAPI.Validation
{
    public class TeacherDetailValidation : AbstractValidator<TeacherDetail>
    {
        public TeacherDetailValidation()
        {
            

            RuleFor(teacher => teacher.Qualification)
                .NotEmpty()
                .WithMessage("Qualification is required.")
                .MaximumLength(100)
                .WithMessage("Qualification must be 100 characters or less.");

            RuleFor(teacher => teacher.ExperienceYears)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Experience must be 0 years or more.");
        }
    }
}
