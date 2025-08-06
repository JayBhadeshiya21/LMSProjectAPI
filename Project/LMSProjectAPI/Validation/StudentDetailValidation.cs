using FluentValidation;

namespace LMSProjectAPI.Validation
{
    public class StudentDetailValidation : AbstractValidator<StudentDetail>
    {
        public StudentDetailValidation()
        {

            RuleFor(student => student.EnrollmentNumber)
                .NotEmpty()
                .WithMessage("Enrollment Number is required.")
                .MaximumLength(50)
                .WithMessage("Enrollment Number must be 50 characters or less.");

            RuleFor(student => student.CourseStream)
                .NotEmpty()
                .WithMessage("Course Stream is required.")
                .MaximumLength(100)
                .WithMessage("Course Stream must be 100 characters or less.");
        }
    }
}
