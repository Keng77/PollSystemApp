using MediatR;
using PollSystemApp.Application.Common.Dto.PollDtos; 
using PollSystemApp.Application.Common.Responses;    

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls
{
    public class GetAllPollsQuery : IRequest<ApiBaseResponse>
    {
    }
}