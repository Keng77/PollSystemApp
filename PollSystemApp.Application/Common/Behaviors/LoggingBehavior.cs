using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("Handling {RequestName}. Request data: {@Request}", requestName, request);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();
                stopwatch.Stop();
                _logger.LogInformation("Handled {RequestName} successfully in {ElapsedMilliseconds}ms.", requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An error occurred while handling {RequestName} after {ElapsedMilliseconds}ms.", requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}