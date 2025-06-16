using MediatR; 
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll; 
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollById;   
using PollSystemApp.Application.Common.Dto.PollDtos;                  
using System;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;

namespace PollSystemApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PollsController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [HttpPost]
        // [Authorize(Roles = "Admin,User")] 
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollCommand command)
        {
            var pollId = await _mediator.Send(command);

            var createdPoll = await _mediator.Send(new GetPollByIdQuery { Id = pollId });
            if (createdPoll == null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetPollById), new { id = pollId }, createdPoll);
        }

        [HttpGet("{id:guid}")]
        // [Authorize]
        public async Task<IActionResult> GetPollById(Guid id)
        {
            var query = new GetPollByIdQuery { Id = id };
            var pollDto = await _mediator.Send(query);

            if (pollDto == null)
            {
                return NotFound();
            }

            return Ok(pollDto);
        }

    }
}