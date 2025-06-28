using FluentValidation;
using MediatR;
using PollSystemApp.Domain.Common.Exceptions;

namespace PollSystemApp.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> :
        IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);


            var validationFailures = _validators
                .Select(validator => validator.Validate(context))
                .SelectMany(validationResult => validationResult.Errors)
                .Where(failure => failure != null)
                .ToList();

            if (validationFailures.Count > 0)
            {

                var errorsDictionary = validationFailures
                    .GroupBy(
                        x => x.PropertyName,
                        x => x.ErrorMessage,
                        (propertyName, errorMessages) => new
                        {
                            Key = propertyName,
                            Values = errorMessages.Distinct().ToArray()
                        })
                    .ToDictionary(x => x.Key, x => x.Values);

                throw new ValidationAppException(errorsDictionary);
            }

            return await next();
        }
    }
}