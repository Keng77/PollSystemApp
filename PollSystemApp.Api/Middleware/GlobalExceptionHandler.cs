using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Domain.Common.Exceptions;
using System.Net;
using System.Text.Json; 

namespace PollSystemApp.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "GlobalExceptionHandler caught an exception: {ErrorMessage}", exception.Message);

            httpContext.Response.ContentType = "application/problem+json";

            (int statusCode, string title) = exception switch
            {
                NotFoundException => ((int)HttpStatusCode.NotFound, "Resource Not Found"),
                BadRequestException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
                ValidationAppException => ((int)HttpStatusCode.UnprocessableEntity, "Validation Error"),
                ForbiddenAccessException => ((int)HttpStatusCode.Forbidden, "Forbidden Access"),
                _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            httpContext.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = exception.GetType().Name,
                Instance = httpContext.Request.Path
            };

            if (exception is ValidationAppException valEx)
            {
                problemDetails.Extensions["errors"] = valEx.Errors;
                problemDetails.Detail = "One or more validation errors occurred. See 'errors' for details.";
            }
            else
            {
                problemDetails.Detail = (statusCode == (int)HttpStatusCode.InternalServerError && !httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
                    ? "An unexpected error occurred. Please try again later."
                    : exception.Message;
            }

            var jsonResponse = JsonSerializer.Serialize(problemDetails);
            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

            return true;
        }
    }
}