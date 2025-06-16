using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Api.Extensions
{
    public static class ApiBaseResponseExtensions
    {
        
        public static TResult GetResult<TResult>(this ApiBaseResponse? apiBaseResponse) 
        {
            if (apiBaseResponse == null)
            {
                throw new ArgumentNullException(nameof(apiBaseResponse), "API response cannot be null when attempting to get result.");
            }

            if (apiBaseResponse is ApiOkResponse<TResult> okResponse)
            {
                 return okResponse.Result;
            }
            throw new InvalidCastException($"Cannot get result of type {typeof(TResult).Name} from response of type {apiBaseResponse.GetType().Name}. Expected ApiOkResponse<{typeof(TResult).Name}>.");
        }
    }
}