using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Dto.PollResultDtos;
using PollSystemApp.Application.Common.Dto.VoteDtos;
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
using System.Text.Json;

namespace PollSystemApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PollsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PollsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Polls
    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(PollDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollCommand command, CancellationToken cancellationToken)
    {
        var pollId = await _mediator.Send(command, cancellationToken);
        var getPollQuery = new GetPollByIdQuery { Id = pollId };
        var createdPollResponse = await _mediator.Send(getPollQuery, cancellationToken);
        return CreatedAtAction(nameof(GetPollById), new { id = pollId }, createdPollResponse);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PollDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPollById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPollByIdQuery { Id = id };
        var pollDto = await _mediator.Send(query, cancellationToken);
        return Ok(pollDto);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PollDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPolls([FromQuery] GetAllPollsQuery query, CancellationToken cancellationToken)
    {
        var pagedResponse = await _mediator.Send(query, cancellationToken);
        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagedResponse.MetaData, new JsonSerializerOptions
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        return Ok(pagedResponse.Items);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePoll(Guid id, [FromBody] PollForUpdateDto pollUpdateDto, CancellationToken cancellationToken)
    {
        var command = new UpdatePollCommand { Id = id, PollData = pollUpdateDto };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePoll(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePollCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Options 
    [HttpPost("{pollId:guid}/options")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(typeof(OptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddOptionToPoll(Guid pollId, [FromBody] OptionForCreationDto optionData, CancellationToken cancellationToken)
    {
        var command = new AddOptionToPollCommand { PollId = pollId, OptionData = optionData };
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPollOptionById), new { pollId = pollId, optionId = response.Id }, response);
    }

    [HttpGet("{pollId:guid}/options/{optionId:guid}", Name = "GetPollOptionById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPollOptionById(Guid pollId, Guid optionId, CancellationToken cancellationToken)
    {
        var query = new GetPollOptionByIdQuery { PollId = pollId, OptionId = optionId };
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{pollId:guid}/options/{optionId:guid}")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdatePollOption(Guid pollId, Guid optionId, [FromBody] OptionForUpdateDto optionUpdateData, CancellationToken cancellationToken)
    {
        var command = new UpdatePollOptionCommand { PollId = pollId, OptionId = optionId, OptionData = optionUpdateData };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{pollId:guid}/options/{optionId:guid}")]
    [Authorize(Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePollOption(Guid pollId, Guid optionId, CancellationToken cancellationToken)
    {
        var command = new DeletePollOptionCommand(pollId, optionId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Votes 
    [HttpPost("{pollId:guid}/vote")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CastVote(Guid pollId, [FromBody] CreateVoteRequestDto requestBody, CancellationToken cancellationToken)
    {
        var command = new CreateVoteCommand { PollId = pollId, OptionIds = requestBody.OptionIds };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpGet("{pollId:guid}/vote-status")]
    [ProducesResponseType(typeof(UserVoteStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckUserVoteStatus(Guid pollId, CancellationToken cancellationToken)
    {
        var query = new CheckUserVoteQuery { PollId = pollId };
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    // Poll Management / Results
    [HttpPost("{pollId:guid}/end-early")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndPollEarly(Guid pollId, CancellationToken cancellationToken)
    {
        var command = new EndPollEarlyCommand(pollId);
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpGet("{pollId:guid}/results")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PollResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPollResults(Guid pollId, CancellationToken cancellationToken)
    {
        var query = new GetPollResultsQuery { PollId = pollId };
        var response = await _mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{pollId:guid}/results/export-csv")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportPollResultsToCsv(Guid pollId, CancellationToken cancellationToken)
    {
        var query = new ExportPollResultsToCsvQuery { PollId = pollId };
        var exportData = await _mediator.Send(query, cancellationToken);
        if (exportData.FileContents == null || exportData.FileContents.Length == 0)
        {
            return NotFound(new ProblemDetails { Title = "No data to export or error generating CSV.", Status = StatusCodes.Status404NotFound });
        }
        return File(exportData.FileContents, exportData.ContentType, exportData.FileName);
    }
}