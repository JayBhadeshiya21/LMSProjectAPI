using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace LMSProjectAPI.Validation
{
    public class FeedbackValidation : AbstractValidator<Feedback>
    {
        public FeedbackValidation()
        {
            RuleFor(feedback => feedback.StudentId)
                .NotEmpty()
                .WithMessage("Student ID is required.");

            RuleFor(feedback => feedback.CourseId)
                .NotEmpty()
                .WithMessage("Course ID is required.");

            RuleFor(feedback => feedback.Comment)
                .NotEmpty()
                .WithMessage("Comment is required.")
                .MaximumLength(1000)
                .WithMessage("Comment cannot exceed 1000 characters.");

            RuleFor(feedback => feedback.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");
        }
    }
}
