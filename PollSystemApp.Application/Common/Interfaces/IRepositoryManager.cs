using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IRepositoryManager
    {
        IUserRepository Users { get; }
        IOptionRepository Options { get; }
        IOptionVoteSummaryRepository OptionVoteSummaries { get; }
        IPollRepository Polls { get; }
        IPollResultRepository PollResults { get; }
        ITagRepository Tags { get; }
        IVoteRepository Votes { get; }

        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }

}
