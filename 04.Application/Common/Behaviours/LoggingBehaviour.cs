using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Smlp.Application.Common.Extensions;
using Smlp.Application.Services.CurrentUser;
using Smlp.Shared.Common.Constants;

namespace Smlp.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUser;

    public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var formattedRequest = request.ToPrettyJson();
        var username = _currentUser.Username;
        var ipAddress = _currentUser.IpAddress;

        _logger.LogInformation("{RequestName} by {Username} from {IpAddress}.\n{RequestName}\n{RequestObject}",
           requestName, username, ipAddress, requestName, formattedRequest);

        return Task.CompletedTask;
    }
}
