using System;

namespace PollSystemApp.Domain.Common.Exceptions
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException() : base("Access to the resource is forbidden.") { }
        public ForbiddenAccessException(string message) : base(message) { }
        public ForbiddenAccessException(string message, Exception innerException) : base(message, innerException) { }
    }
}