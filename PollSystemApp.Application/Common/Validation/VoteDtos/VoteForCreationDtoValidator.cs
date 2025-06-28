using FluentValidation;
using PollSystemApp.Application.Common.Dto.VoteDtos;

namespace PollSystemApp.Application.Common.Validation.VoteDtos
{
    public class VoteForCreationDtoValidator : AbstractValidator<VoteForCreationDto>
    {
        public VoteForCreationDtoValidator()
        {
            RuleFor(v => v.PollId)
                .NotEmpty().WithMessage("Poll ID is required.");

            RuleFor(v => v.OptionIds)
                .NotEmpty().WithMessage("At least one Option ID must be provided.")
                .Must(optionIds => optionIds != null && optionIds.Count > 0)
                .WithMessage("At least one Option ID must be selected.");


            RuleForEach(v => v.OptionIds)
                .NotEmpty().WithMessage("Option ID cannot be empty.");
        }
    }
}