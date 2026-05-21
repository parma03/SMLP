using MediatR;
using Microsoft.Extensions.Logging;
using Smlp.Application.Common.Extensions;
using Smlp.Application.Services.CurrentUser;

namespace Smlp.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUser;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            var requestName = typeof(TRequest).Name;
            var formattedRequest = request.ToPrettyJson();
            var username = _currentUser.Username;
            var ipAddress = _currentUser.IpAddress;

            _logger.LogError(exception, "Unhandled Exception when executing {RequestName} by {Username} from {IpAddress}.\n{RequestName}\n{RequestObject}",
               requestName, username, ipAddress, requestName, formattedRequest);

            throw;
        }
    }
}
