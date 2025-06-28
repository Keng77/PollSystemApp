using FluentValidation;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Validation.OptionDtos;

namespace PollSystemApp.Application.Common.Validation.PollDtos
{
    public class PollForCreationDtoValidator : AbstractValidator<PollForCreationDto>
    {
        public PollForCreationDtoValidator()
        {
            RuleFor(p => p.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(p => p.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(p => p.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date cannot be in the past.");

            RuleFor(p => p.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(p => p.StartDate).WithMessage("End date must be after start date.");

            RuleFor(p => p.Options)
                .NotEmpty().WithMessage("At least one option is required.")
                .Must(options => options.Count >= 2).WithMessage("A poll must have at least two options.");

            RuleForEach(p => p.Options).SetValidator(new OptionForCreationDtoValidator());
        }
    }
}