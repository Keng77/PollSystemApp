using Microsoft.AspNetCore.Identity;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Users;
using PollSystemApp.Infrastructure.Polls.Persistence;
using PollSystemApp.Infrastructure.Users.Persistence;


namespace PollSystemApp.Infrastructure.Common.Persistence
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly Lazy<IUserRepository> _userRepository;
        private readonly Lazy<IOptionRepository> _optionRepository;
        private readonly Lazy<IOptionVoteSummaryRepository> _optionVoteSummaryRepository;
        private readonly Lazy<IPollRepository> _pollRepository;
        private readonly Lazy<IPollResultRepository> _pollResultRepository;
        private readonly Lazy<ITagRepository> _tagRepository;
        private readonly Lazy<IVoteRepository> _voteRepository;

        public IUserRepository Users => _userRepository.Value;
        public IOptionRepository Options => _optionRepository.Value;
        public IOptionVoteSummaryRepository OptionVoteSummaries => _optionVoteSummaryRepository.Value;
        public IPollRepository Polls => _pollRepository.Value;
        public IPollResultRepository PollResults => _pollResultRepository.Value;
        public ITagRepository Tags => _tagRepository.Value;
        public IVoteRepository Votes => _voteRepository.Value;

        public RepositoryManager(AppDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userRepository = new Lazy<IUserRepository>(() => new UserRepository(_userManager, _dbContext));
            _optionRepository = new Lazy<IOptionRepository>(() => new OptionRepository(_dbContext));
            _optionVoteSummaryRepository = new Lazy<IOptionVoteSummaryRepository>(() => new OptionVoteSummaryRepository(_dbContext));
            _pollRepository = new Lazy<IPollRepository>(() => new PollRepository(_dbContext));
            _pollResultRepository = new Lazy<IPollResultRepository>(() => new PollResultRepository(_dbContext));
            _tagRepository = new Lazy<ITagRepository>(() => new TagRepository(_dbContext));
            _voteRepository = new Lazy<IVoteRepository>(() => new VoteRepository(_dbContext));

        }



        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

}
