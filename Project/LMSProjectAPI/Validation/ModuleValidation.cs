using FluentValidation;

namespace LMSProjectAPI.Validation
{
    public class ModuleValidation : AbstractValidator<Module>
    {
        public ModuleValidation()
        {

            RuleFor(module => module.Title)
                .NotEmpty()
                .WithMessage("Module title is required.")
                .MaximumLength(200)
                .WithMessage("Title must be 200 characters or less.");

            RuleFor(module => module.Content)
                .NotEmpty()
                .WithMessage("Module content is required.");

            RuleFor(module => module.VideoUrl)
                .NotEmpty()
                .WithMessage("Video URL is required.");


            RuleFor(module => module.OrderIndex)
                .NotNull()
                .WithMessage("Order Index is required.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Order Index must be 0 or greater.");
        }
    }
}
