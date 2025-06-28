using PollSystemApp.Domain.Polls;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IPollResultsCalculator
    {
        Task<PollResult> CalculateResultsAsync(Poll poll, CancellationToken cancellationToken);
    }
}