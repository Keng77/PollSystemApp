using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Application.Common.Responses; 


namespace PollSystemApp.Api.Controllers
{
    [ApiController]
   
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult ProcessError(ApiBaseResponse response)
        {
          
            if (response is ApiErrorResponse errorResponse) 
            {
                 return BadRequest(new { errorResponse.Message, errorResponse.Errors });
            }

            return BadRequest("An unexpected error occurred processing the response from the application layer.");
        }
    }
}