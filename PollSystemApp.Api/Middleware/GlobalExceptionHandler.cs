using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Domain.Common.Exceptions;
using System.Net;

namespace PollSystemApp.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {ErrorMessage}", exception.Message);

            (int statusCode, string title, string? detail, IReadOnlyDictionary<string, string[]>? validationErrors) = exception switch
            {
                NotFoundException ex => ((int)HttpStatusCode.NotFound, "Resource Not Found", ex.Message, null),
                BadRequestException ex => ((int)HttpStatusCode.BadRequest, "Bad Request", ex.Message, null),
                ValidationAppException ex => ((int)HttpStatusCode.UnprocessableEntity, "Validation Error", "One or more validation errors occurred.", ex.Errors),
                ForbiddenAccessException ex => ((int)HttpStatusCode.Forbidden, "Forbidden", ex.Message, null),
                _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred. Please try again later.", null)
            };


            httpContext.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = exception.GetType().Name,
                Instance = httpContext.Request.Path
            };

            if (exception is ValidationAppException valEx)
            {
                problemDetails.Extensions["errors"] = valEx.Errors;
                problemDetails.Detail = "One or more validation errors occurred. See 'errors' property for details.";
            }
            else if (problemDetails.Status == (int)HttpStatusCode.InternalServerError && httpContext.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == false)
            {
                problemDetails.Detail = "An unexpected error occurred. Please try again later.";
                problemDetails.Title = "Internal Server Error";
            }
            else
            {
                problemDetails.Detail = exception.Message;
            }


            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
        }
    }
}