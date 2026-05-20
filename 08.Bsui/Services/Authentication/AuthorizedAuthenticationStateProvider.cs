using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;

namespace Smlp.Bsui.Services.Authentication;

public class AuthorizedAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "smlp_access_token";
    private const string RefreshTokenKey = "smlp_refresh_token";
    private const string UserInfoKey = "smlp_user_info";

    private readonly ProtectedLocalStorage _localStorage;
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly ILogger<AuthorizedAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public AuthorizedAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AuthenticationOptions> authenticationOptions,
        ProtectedLocalStorage localStorage,
        ILogger<AuthorizedAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _authenticationOptions = authenticationOptions.Value;
        _localStorage = localStorage;
        _logger = logger;

        // Mode None: grant full access for development
        if (_authenticationOptions.Provider == "None")
        {
            var devClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"),
                new(ClaimTypes.Name, "admin"),
                new("displayname", "Administrator"),
                new(ClaimTypes.Role, "Admin"),
                new(ClaimTypes.Email, "admin@smlp.local")
            };
            var devIdentity = new ClaimsIdentity(devClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            _currentUser = new ClaimsPrincipal(devIdentity);
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authenticationOptions.Provider == "None")
        {
            return new AuthenticationState(_currentUser);
        }

        try
        {
            var storedToken = await _localStorage.GetAsync<string>(AccessTokenKey);
            if (!storedToken.Success || string.IsNullOrWhiteSpace(storedToken.Value))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(storedToken.Value);
            var expiry = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

            if (expiry is not null)
            {
                var expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
                if (expiryTime < DateTimeOffset.UtcNow)
                {
                    await LogoutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            _currentUser = new ClaimsPrincipal(identity);

            return new AuthenticationState(_currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gagal mendapatkan authentication state.");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task LoginAsync(string accessToken, string refreshToken)
    {
        await _localStorage.SetAsync(AccessTokenKey, accessToken);
        await _localStorage.SetAsync(RefreshTokenKey, refreshToken);

        var claims = ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        _currentUser = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task LogoutAsync()
    {
        await _localStorage.DeleteAsync(AccessTokenKey);
        await _localStorage.DeleteAsync(RefreshTokenKey);
        await _localStorage.DeleteAsync(UserInfoKey);

        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(AccessTokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(RefreshTokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
        catch
        {
            return Enumerable.Empty<Claim>();
        }
    }
}
