using FluentValidation;
using PollSystemApp.Application.Common.Validation.OptionDtos;

namespace PollSystemApp.Application.UseCases.Polls.Commands.AddOptionToPoll
{
    public class AddOptionToPollCommandValidator : AbstractValidator<AddOptionToPollCommand>
    {
        public AddOptionToPollCommandValidator()
        {
            RuleFor(x => x.PollId)
                .NotEmpty().WithMessage("Poll ID is required.");

            RuleFor(x => x.OptionData)
                .NotNull().WithMessage("Option data is required.")
                .SetValidator(new OptionForCreationDtoValidator());
        }
    }
}