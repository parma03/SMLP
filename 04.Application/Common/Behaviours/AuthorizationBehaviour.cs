using System.Reflection;
using MediatR;
using Microsoft.Extensions.Options;
using Smlp.Application.Common.Attributes;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Authentication;
using Smlp.Application.Services.Authorization;
using Smlp.Application.Services.CurrentUser;
using Smlp.Shared.Services.Authentication.Constants;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private const string StartingErrorMessage = "The server is refusing to process the request because the user";
    private readonly bool _usingAuthentication;
    private readonly bool _usingAuthorization;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationBehaviour(
        IOptions<AuthenticationOptions> authenticationOptions,
        IOptions<AuthorizationOptions> authorizationOptions,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService)
    {
        _usingAuthentication = authenticationOptions.Value.Provider != AuthenticationProvider.None;
        _usingAuthorization = authorizationOptions.Value.Provider != "None";
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_usingAuthentication)
        {
            return await next();
        }

        if (!_usingAuthorization)
        {
            return await next();
        }

        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (!authorizeAttributes.Any())
        {
            return await next();
        }

        if (!_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException($"{StartingErrorMessage} is not authenticated.");
        }

        var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));

        if (!authorizeAttributesWithPolicies.Any())
        {
            return await next();
        }

        if (string.IsNullOrWhiteSpace(_currentUserService.PositionId))
        {
            throw new ForbiddenAccessException($"{StartingErrorMessage} {_currentUserService.Username} does not have a Position ID.");
        }

        var authorizationInfo = await _authorizationService.GetAuthorizationInfoAsync(_currentUserService.PositionId, cancellationToken);

        foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
        {
            var authorized = authorizationInfo.Roles.SelectMany(x => x.Permissions).Any(x => x.Equals(policy, StringComparison.OrdinalIgnoreCase));

            if (!authorized)
            {
                throw new ForbiddenAccessException($"{StartingErrorMessage} {_currentUserService.Username} with Position ID {_currentUserService.PositionId} does not have the following {AuthorizationClaimTypes.Permission}: {policy}");
            }
        }

        return await next();
    }
}
