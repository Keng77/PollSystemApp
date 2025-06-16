namespace PollSystemApp.Application.Common.Responses
{
    public abstract class ApiBaseResponse(bool success)
    {
        public bool Success { get; protected set; } = success;
    }
}