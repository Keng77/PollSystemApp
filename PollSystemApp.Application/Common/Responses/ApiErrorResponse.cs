using System.Collections.Generic;

namespace PollSystemApp.Application.Common.Responses
{
    public class ApiErrorResponse(string message, IEnumerable<string>? errors = null) : ApiBaseResponse(false)
    {
        public string Message { get; } = message;
        public IEnumerable<string>? Errors { get; } = errors;
    }
}