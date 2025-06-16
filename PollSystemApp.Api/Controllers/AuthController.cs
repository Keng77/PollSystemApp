using MediatR;
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Application.UseCases.Auth.Commands.LoginUser;
using PollSystemApp.Application.UseCases.Auth.Commands.RegisterUser;
using PollSystemApp.Application.Common.Dto.UserDtos; 
using PollSystemApp.Api.Extensions; 
using System.Threading.Tasks;

namespace PollSystemApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok();
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response.GetResult<AuthResponseDto>());
        }

    }
}