using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Api.Extensions; 
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll;
using PollSystemApp.Application.UseCases.Polls.Commands.DeletePoll;
using PollSystemApp.Application.UseCases.Polls.Commands.UpdatePoll;
using PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollById;
using System;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> GetAllPolls([FromQuery] GetAllPollsQuery query) 
        {
            var response = await _mediator.Send(query);
            return Ok(response.GetResult<List<PollDto>>());
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdatePoll(Guid id, [FromBody] PollForUpdateDto pollUpdateDto)
        {
            var command = new UpdatePollCommand { Id = id, PollData = pollUpdateDto };
            await _mediator.Send(command); 
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeletePoll(Guid id)
        {
            var command = new DeletePollCommand(id);
            await _mediator.Send(command); 
            return NoContent();
        }
    }
}