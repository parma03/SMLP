using System.Security.Claims;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Services.Authentication.IdAMan;
using Smlp.Bsui.Services.Authentication.IS4IM;
using Smlp.Bsui.Services.Authorization;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Services.Authentication.Constants;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Bsui.Services.Authentication;

public class AuthorizedAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _user;
    private readonly string _tokenUrl = default!;
    private readonly string _clientId = default!;
    private readonly string _clientSecret = default!;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly UserInfoService _userInfo;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly bool _usingAuthentication;
    private readonly bool _usingAuthorization;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizedAuthenticationStateProvider> _logger;

    public AuthorizedAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AuthenticationOptions> authenticationOptions,
        IOptions<AuthorizationOptions> authorizationOptions,
        IConfiguration configuration,
        ProtectedLocalStorage localStorage,
        UserInfoService userInfo,
        IAuthorizationService authorizationService,
        ILogger<AuthorizedAuthenticationStateProvider> logger)
    {
        _authenticationOptions = authenticationOptions.Value;
        _usingAuthentication = _authenticationOptions.Provider is not AuthenticationProvider.None;
        _usingAuthorization = authorizationOptions.Value.Provider is not AuthorizationProvider.None;

        if (!_usingAuthentication)
        {
            var claims = Permissions.All
                .Select(permission => new Claim(AuthorizationClaimTypes.Permission, permission))
                .ToList();

            var noneAuthenticationIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            _user = new ClaimsPrincipal(noneAuthenticationIdentity);
        }
        else
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                throw new InvalidOperationException("HttpContext object is null.");
            }

            _user = httpContext.User;

            if (!_usingAuthorization)
            {
                var claims = new List<Claim>();

                foreach (var claim in _user.Claims)
                {
                    if (claim.Type
                        is OidcConstants.TokenResponse.AccessToken
                        or OidcConstants.TokenResponse.RefreshToken
                        or AuthorizationClaimTypes.PositionId
                        or AuthorizationClaimTypes.PositionName
                        or AuthorizationClaimTypes.Permission)
                    {
                        continue;
                    }
                    else if (claim.Type.StartsWith(AuthorizationClaimTypes.CustomParameter))
                    {
                        continue;
                    }
                    else
                    {
                        claims.Add(claim);
                    }
                }

                foreach (var permission in Permissions.All)
                {
                    claims.Add(new Claim(AuthorizationClaimTypes.Permission, permission));
                }

                var noneAuthorizationIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                _user = new ClaimsPrincipal(noneAuthorizationIdentity);
            }
        }

        switch (_authenticationOptions.Provider)
        {
            case AuthenticationProvider.None:
                _tokenUrl = string.Empty;
                _clientId = string.Empty;
                _clientSecret = string.Empty;
                break;

            case AuthenticationProvider.IdAMan:
                var idAManAuthenticationOptions = configuration.GetSection(IdAManAuthenticationOptions.SectionKey).Get<IdAManAuthenticationOptions>();
                _tokenUrl = idAManAuthenticationOptions.TokenUrl;
                _clientId = idAManAuthenticationOptions.ClientId;
                _clientSecret = idAManAuthenticationOptions.ClientSecret;
                break;

            case AuthenticationProvider.IS4IM:
                var is4imAuthenticationOptions = configuration.GetSection(IS4IMAuthenticationOptions.SectionKey).Get<IS4IMAuthenticationOptions>();
                _tokenUrl = is4imAuthenticationOptions.TokenUrl;
                _clientId = is4imAuthenticationOptions.ClientId;
                _clientSecret = is4imAuthenticationOptions.ClientSecret;
                break;

            default:
                throw new ArgumentException($"{CommonDisplayTextFor.Unsupported} {AuthenticationDisplayTextFor.AuthenticationProvider}: {authenticationOptions.Value.Provider}");
        }

        _localStorage = localStorage;
        _userInfo = userInfo;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = _user.Identity;

        if (!_usingAuthentication)
        {
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(_user)));
        }

        if (identity is null)
        {
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
        }

        if (!identity.IsAuthenticated)
        {
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(_user)));
    }

    public async Task LoadIdentity()
    {
        if (!_usingAuthentication)
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));

            return;
        }

        var identity = _user.Identity;

        if (identity is null || !identity.IsAuthenticated)
        {
            await ClearStoredIdentityAsync();

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));

            return;
        }

        if (!_usingAuthorization)
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));

            return;
        }

        var storedAccessToken = await _localStorage.GetAsync<string>(OidcConstants.TokenResponse.AccessToken);
        var storedRefreshToken = await _localStorage.GetAsync<string>(OidcConstants.TokenResponse.RefreshToken);
        var storedPositionId = await _localStorage.GetAsync<string>(AuthorizationClaimTypes.PositionId);
        var storedPositionName = await _localStorage.GetAsync<string>(AuthorizationClaimTypes.PositionName);
        var storedPermissions = await _localStorage.GetAsync<IList<string>>(AuthorizationClaimTypes.Permission);
        var storedCustomParameters = await _localStorage.GetAsync<IDictionary<string, string>>(AuthorizationClaimTypes.CustomParameter);

        var claims = new List<Claim>();

        foreach (var claim in _user.Claims)
        {
            if (claim.Type
                is OidcConstants.TokenResponse.AccessToken
                or OidcConstants.TokenResponse.RefreshToken
                or AuthorizationClaimTypes.PositionId
                or AuthorizationClaimTypes.PositionName
                or AuthorizationClaimTypes.Permission)
            {
                continue;
            }
            else if (claim.Type.StartsWith(AuthorizationClaimTypes.CustomParameter))
            {
                continue;
            }
            else
            {
                claims.Add(claim);
            }
        }

        if (!string.IsNullOrWhiteSpace(storedAccessToken.Value))
        {
            claims.Add(new Claim(OidcConstants.TokenResponse.AccessToken, storedAccessToken.Value));

            _userInfo.AccessToken = storedAccessToken.Value;
        }
        else
        {
            var claimAccessToken = _user.GetAccessToken();

            if (!string.IsNullOrWhiteSpace(claimAccessToken))
            {
                claims.Add(new Claim(OidcConstants.TokenResponse.AccessToken, claimAccessToken));

                _userInfo.AccessToken = claimAccessToken;
            }
        }

        if (!string.IsNullOrWhiteSpace(storedRefreshToken.Value))
        {
            claims.Add(new Claim(OidcConstants.TokenResponse.RefreshToken, storedRefreshToken.Value));
        }
        else
        {
            claims.Add(new Claim(OidcConstants.TokenResponse.RefreshToken, _user.GetRefreshToken()!));
        }

        if (!string.IsNullOrWhiteSpace(storedPositionId.Value))
        {
            claims.Add(new Claim(AuthorizationClaimTypes.PositionId, storedPositionId.Value));

            _userInfo.PositionId = storedPositionId.Value;
        }
        else
        {
            var claimPositionId = _user.GetPositionId();

            if (claimPositionId is not null)
            {
                claims.Add(new Claim(AuthorizationClaimTypes.PositionId, claimPositionId));

                _userInfo.PositionId = claimPositionId;
            }
            else
            {
                _userInfo.PositionId = null;
            }
        }

        if (!string.IsNullOrWhiteSpace(storedPositionName.Value))
        {
            claims.Add(new Claim(AuthorizationClaimTypes.PositionName, storedPositionName.Value));
        }
        else
        {
            var claimPositionName = _user.GetPositionName();

            if (claimPositionName is not null)
            {
                claims.Add(new Claim(AuthorizationClaimTypes.PositionName, claimPositionName));
            }
        }

        if (storedPermissions.Value is not null)
        {
            foreach (var permission in storedPermissions.Value)
            {
                claims.Add(new Claim(AuthorizationClaimTypes.Permission, permission));
            }
        }
        else
        {
            foreach (var permission in _user.GetPermissions())
            {
                claims.Add(new Claim(AuthorizationClaimTypes.Permission, permission));
            }
        }

        if (storedCustomParameters.Value is not null)
        {
            foreach (var customParameter in storedCustomParameters.Value)
            {
                claims.Add(new Claim(customParameter.Key, customParameter.Value));
            }
        }
        else
        {
            foreach (var customParameter in _user.GetCustomParameters())
            {
                claims.Add(new Claim(customParameter.Key, customParameter.Value));
            }
        }

        var newIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(newIdentity))));
    }

    public async Task LoadAuthorizationAsync(string positionId, string positionName)
    {
        var identity = new ClaimsIdentity(_user.Claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authorizationInfo = await _authorizationService.GetAuthorizationInfoAsync(positionId, _userInfo.AccessToken);

        if (authorizationInfo is null)
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(_user))));

            return;
        }

        _userInfo.PositionId = positionId;

        var claims = new List<Claim>();

        foreach (var claim in _user.Claims)
        {
            if (claim.Type
                is AuthorizationClaimTypes.PositionId
                or AuthorizationClaimTypes.PositionName
                or AuthorizationClaimTypes.Permission)
            {
                continue;
            }
            else if (claim.Type.StartsWith(AuthorizationClaimTypes.CustomParameter))
            {
                continue;
            }
            else
            {
                claims.Add(claim);
            }
        }

        claims.Add(new Claim(AuthorizationClaimTypes.PositionId, positionId));
        await _localStorage.SetAsync(AuthorizationClaimTypes.PositionId, positionId);

        claims.Add(new Claim(AuthorizationClaimTypes.PositionName, positionName));
        await _localStorage.SetAsync(AuthorizationClaimTypes.PositionName, positionName);

        var distinctPermissions = authorizationInfo.Roles.SelectMany(x => x.Permissions).Distinct();

        foreach (var permission in distinctPermissions)
        {
            if (!claims.Any(x => x.Type == AuthorizationClaimTypes.Permission && x.Value == permission))
            {
                claims.Add(new Claim(AuthorizationClaimTypes.Permission, permission));
            }
        }

        await _localStorage.SetAsync(AuthorizationClaimTypes.Permission, distinctPermissions);

        var customParameters = new Dictionary<string, string>();

        foreach (var customParameter in authorizationInfo.CustomParameters)
        {
            var customParameterClaimType = $"{AuthorizationClaimTypes.CustomParameter}{AuthorizationDelimiterFor.CustomParameter}{customParameter.Key}";

            if (!claims.Any(x => x.Type == customParameterClaimType && x.Value == customParameter.Value))
            {
                claims.Add(new Claim(customParameterClaimType, customParameter.Value));
                customParameters.Add(customParameterClaimType, customParameter.Value);
            }
        }

        await _localStorage.SetAsync(AuthorizationClaimTypes.CustomParameter, customParameters);

        identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task RefreshTokensAsync()
    {
        var storedRefreshToken = await _localStorage.GetAsync<string>(OidcConstants.TokenResponse.RefreshToken);
        var refreshToken = string.Empty;

        if (storedRefreshToken.Value is not null)
        {
            refreshToken = storedRefreshToken.Value;
        }
        else
        {
            var claimRefreshToken = _user.GetRefreshToken();

            if (!string.IsNullOrWhiteSpace(claimRefreshToken))
            {
                refreshToken = claimRefreshToken;
            }
        }

        var tokenClientOptions = new TokenClientOptions
        {
            Address = _tokenUrl,
            ClientId = _clientId,
            ClientSecret = _clientSecret
        };

        var httpClient = new HttpClient();
        var tokenClient = new TokenClient(httpClient, tokenClientOptions);
        var tokenResponse = await tokenClient.RequestRefreshTokenAsync(refreshToken);

        if (tokenResponse.IsError)
        {
            _logger.LogError("Error in requesting new Access Token using Refresh Token {RefreshToken}", refreshToken);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(_user))));

            return;
        }

        var claims = new List<Claim>();

        foreach (var claim in _user.Claims)
        {
            if (claim.Type is OidcConstants.TokenResponse.AccessToken or OidcConstants.TokenResponse.RefreshToken)
            {
                continue;
            }
            else
            {
                claims.Add(claim);
            }
        }

        claims.Add(new Claim(OidcConstants.TokenResponse.AccessToken, tokenResponse.AccessToken));
        await _localStorage.SetAsync(OidcConstants.TokenResponse.AccessToken, tokenResponse.AccessToken);
        _userInfo.AccessToken = tokenResponse.AccessToken;

        claims.Add(new Claim(OidcConstants.TokenResponse.RefreshToken, tokenResponse.RefreshToken));
        await _localStorage.SetAsync(OidcConstants.TokenResponse.RefreshToken, tokenResponse.RefreshToken);

        var newIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(newIdentity))));
    }

    public async Task ClearStoredIdentityAsync()
    {
        await _localStorage.DeleteAsync(AuthorizationClaimTypes.PositionId);
        await _localStorage.DeleteAsync(AuthorizationClaimTypes.PositionName);
        await _localStorage.DeleteAsync(AuthorizationClaimTypes.Permission);
        await _localStorage.DeleteAsync(AuthorizationClaimTypes.CustomParameter);

        await _localStorage.DeleteAsync(OidcConstants.TokenResponse.AccessToken);
        await _localStorage.DeleteAsync(OidcConstants.TokenResponse.RefreshToken);
    }
}
