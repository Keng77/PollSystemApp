using FluentValidation;
using PollSystemApp.Application.Common.Dto.OptionDtos;

namespace PollSystemApp.Application.Common.Validation.OptionDtos
{
    public class OptionForCreationDtoValidator : AbstractValidator<OptionForCreationDto>
    {
        public OptionForCreationDtoValidator()
        {
            RuleFor(o => o.Text)
                .NotEmpty().WithMessage("Option text is required.")
                .MaximumLength(200).WithMessage("Option text must not exceed 200 characters.");

            RuleFor(o => o.Order)
                .GreaterThanOrEqualTo(0).WithMessage("Option order must be a non-negative number.");
        }
    }
}