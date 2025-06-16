namespace PollSystemApp.Application.Common.Responses
{
    public sealed class ApiOkResponse<TResult>(TResult result) : ApiBaseResponse(true)
    {
        public TResult Result { get; } = result;
    }

    public sealed class ApiOkResponse : ApiBaseResponse
    {
        public ApiOkResponse() : base(true) { }
    }
}