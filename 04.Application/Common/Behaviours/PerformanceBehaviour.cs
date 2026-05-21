using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Smlp.Application.Common.Extensions;
using Smlp.Application.Services.CurrentUser;

namespace Smlp.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUser;

    public PerformanceBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUser)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 1000)
        {
            var requestName = typeof(TRequest).Name;
            var formattedRequest = request.ToPrettyJson();
            var username = _currentUser.Username;
            var ipAddress = _currentUser.IpAddress;

            if (elapsedMilliseconds > 5000)
            {
                _logger.LogError("{RequestName} for ({ElapsedMilliseconds} milliseconds) by {Username} from {IpAddress}.\n{RequestName}\n{RequestObject}",
                   requestName, elapsedMilliseconds, username, ipAddress, requestName, formattedRequest);
            }
            else
            {
                _logger.LogWarning("{RequestName} for ({ElapsedMilliseconds} milliseconds) by {Username} from {IpAddress}.\n{RequestName}\n{RequestObject}",
                   requestName, elapsedMilliseconds, username, ipAddress, requestName, formattedRequest);
            }
        }

        return response;
    }
}
