using MediatR;
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollById;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Api.Extensions; 
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PollSystemApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    public class PollsController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public PollsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollCommand command)
        {
            var response = await _mediator.Send(command);

            var pollId = response.GetResult<Guid>(); 

            var getPollQuery = new GetPollByIdQuery { Id = pollId };
            var createdPollResponse = await _mediator.Send(getPollQuery);

            return CreatedAtAction(nameof(GetPollById), new { id = pollId }, createdPollResponse.GetResult<PollDto>());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPollById(Guid id)
        {
            var query = new GetPollByIdQuery { Id = id };
            var response = await _mediator.Send(query); 

            return Ok(response.GetResult<PollDto>());
        }
    }
}