using FluentValidation;
using PollSystemApp.Application.Common.Dto.PollDtos;
using System;

namespace PollSystemApp.Application.Common.Validation.PollDtos
{
    public class PollForUpdateDtoValidator : AbstractValidator<PollForUpdateDto>
    {
        public PollForUpdateDtoValidator()
        {
            RuleFor(p => p.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(p => p.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(p => p.StartDate)
                .NotEmpty().WithMessage("Start date is required.");
             

            RuleFor(p => p.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(p => p.StartDate).WithMessage("End date must be after start date.");                      
        }
    }
}