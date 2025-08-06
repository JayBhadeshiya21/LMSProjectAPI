using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace LMSProjectAPI.Validation
{
    public class CourseValidation : AbstractValidator<Course>
    {   
        public CourseValidation()
        {
            RuleFor(course => course.Title)
                .NotEmpty()
                .NotNull()
                .WithMessage("Enter Course Title !");

            RuleFor(course => course.Description)
                .NotEmpty()
                .WithMessage("Enter Course Description !");

            RuleFor(course => course.TeacherId)
                .NotEmpty()
                .WithMessage("Enter Teachar Id !");

            RuleFor(course => course.ImageUrl)
                .NotEmpty()
                .WithMessage("Enter Image Url !");

            
            
        }
    }
}
