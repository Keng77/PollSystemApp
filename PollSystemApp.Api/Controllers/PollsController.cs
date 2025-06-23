using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Api.Extensions;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Dto.PollResultDtos;
using PollSystemApp.Application.Common.Dto.VoteDtos;
using PollSystemApp.Application.Common.Pagination;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Application.UseCases.Polls.Commands.AddOptionToPoll;
using PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll;
using PollSystemApp.Application.UseCases.Polls.Commands.DeletePoll;
using PollSystemApp.Application.UseCases.Polls.Commands.DeletePollOption;
using PollSystemApp.Application.UseCases.Polls.Commands.EndPollEarly;
using PollSystemApp.Application.UseCases.Polls.Commands.UpdatePoll;
using PollSystemApp.Application.UseCases.Polls.Commands.UpdatePollOption;
using PollSystemApp.Application.UseCases.Polls.Queries.ExportPollResultsToCsv;
using PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollById;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollOptionById;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollResults;
using PollSystemApp.Application.UseCases.Votes.Commands.CreateVote;
using PollSystemApp.Application.UseCases.Votes.Queries.CheckUserVote;
using System;
using System.Text.Json;
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
            var (items, metaData) = response.GetResult<(List<PollDto> Items, PaginationMetadata MetaData)>();
            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(metaData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            return Ok(items); 
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

        [HttpPost("{pollId:guid}/options")]
        [Authorize(Roles = "Admin,User")] 
        [ProducesResponseType(typeof(OptionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AddOptionToPoll(Guid pollId, [FromBody] OptionForCreationDto optionData)
        {
            var command = new AddOptionToPollCommand { PollId = pollId, OptionData = optionData };
            var response = await _mediator.Send(command);

            var createdOption = response.GetResult<OptionDto>();

            return CreatedAtAction(nameof(GetPollOptionById),
                                   new { pollId = pollId, optionId = createdOption.Id },
                                   createdOption);
        }

        [HttpGet("{pollId:guid}/options/{optionId:guid}", Name = "GetPollOptionById")]
        [Authorize]
        [ProducesResponseType(typeof(OptionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPollOptionById(Guid pollId, Guid optionId)
        {
            var query = new GetPollOptionByIdQuery { PollId = pollId, OptionId = optionId };
            var response = await _mediator.Send(query);
            return Ok(response.GetResult<OptionDto>());
        }

        [HttpPut("{pollId:guid}/options/{optionId:guid}")]
        [Authorize(Roles = "Admin,User")] 
        public async Task<IActionResult> UpdatePollOption(Guid pollId, Guid optionId, [FromBody] OptionForUpdateDto optionUpdateData)
        {
            var command = new UpdatePollOptionCommand
            {
                PollId = pollId,
                OptionId = optionId,
                OptionData = optionUpdateData
            };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{pollId:guid}/options/{optionId:guid}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeletePollOption(Guid pollId, Guid optionId)
        {
            var command = new DeletePollOptionCommand(pollId, optionId);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{pollId:guid}/vote")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CastVote(Guid pollId, [FromBody] CreateVoteRequestDto requestBody)
        {
            var command = new CreateVoteCommand
            {
                PollId = pollId,
                OptionIds = requestBody.OptionIds
            };
            await _mediator.Send(command);
            return Ok(); 
        }

        [HttpGet("{pollId:guid}/vote-status")]
        [Authorize]
        [ProducesResponseType(typeof(UserVoteStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckUserVoteStatus(Guid pollId)
        {
            var query = new CheckUserVoteQuery { PollId = pollId };
            var response = await _mediator.Send(query);
            return Ok(response.GetResult<UserVoteStatusDto>());
        }

        [HttpPost("{pollId:guid}/end-early")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EndPollEarly(Guid pollId)
        {
            var command = new EndPollEarlyCommand(pollId);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpGet("{pollId:guid}/results")]
        [ProducesResponseType(typeof(PollResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPollResults(Guid pollId)
        {
            var query = new GetPollResultsQuery { PollId = pollId };
            var response = await _mediator.Send(query);
            return Ok(response.GetResult<PollResultDto>());
        }

        [HttpGet("{pollId:guid}/results/export-csv")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportPollResultsToCsv(Guid pollId)
        {
            var query = new ExportPollResultsToCsvQuery { PollId = pollId };
            var exportData = await _mediator.Send(query); 

            if (exportData == null || exportData.FileContents == null || exportData.FileContents.Length == 0)
            {
                return NotFound(new ProblemDetails { Title = "No data to export or error generating CSV." });
            }

            return File(exportData.FileContents, exportData.ContentType, exportData.FileName);
        }
    }
}