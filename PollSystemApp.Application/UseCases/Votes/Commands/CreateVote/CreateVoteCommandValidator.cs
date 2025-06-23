using FluentValidation;

namespace PollSystemApp.Application.UseCases.Votes.Commands.CreateVote
{
    public class CreateVoteCommandValidator : AbstractValidator<CreateVoteCommand>
    {
        public CreateVoteCommandValidator()
        {
            RuleFor(v => v.PollId)
                .NotEmpty().WithMessage("Poll ID is required.");

            RuleFor(v => v.OptionIds)
                .NotNull().WithMessage("Option IDs list cannot be null.")
                .NotEmpty().WithMessage("At least one Option ID must be selected to vote.");

            RuleForEach(v => v.OptionIds)
                .NotEmpty().WithMessage("Each Option ID in the list cannot be empty (Guid.Empty).");
        }
    }
}