using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;
using Smlp.Client.Services.BackEnd.Auth;
using Smlp.Shared.Auth.Requests;

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
    private readonly AuthService _authService;
    private readonly NavigationManager _navigationManager;

    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());
    private bool _identityLoaded = false;

    public AuthorizedAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AuthenticationOptions> authenticationOptions,
        ProtectedLocalStorage localStorage,
        ILogger<AuthorizedAuthenticationStateProvider> logger,
        AuthService authService,
        NavigationManager navigationManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authenticationOptions = authenticationOptions.Value;
        _localStorage = localStorage;
        _logger = logger;
        _authService = authService;
        _navigationManager = navigationManager;

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
            _identityLoaded = true;
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authenticationOptions.Provider == "None")
        {
            return new AuthenticationState(_currentUser);
        }

        // If identity has been loaded from storage, return cached user
        if (_identityLoaded)
        {
            return new AuthenticationState(_currentUser);
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    /// <summary>
    /// Must be called from OnAfterRenderAsync (first render) to load tokens from ProtectedLocalStorage.
    /// ProtectedLocalStorage is not available during SSR/prerender.
    /// </summary>
    public async Task LoadIdentity()
    {
        if (_authenticationOptions.Provider == "None")
        {
            _identityLoaded = true;
            return;
        }

        try
        {
            var storedToken = await _localStorage.GetAsync<string>(AccessTokenKey);
            if (!storedToken.Success || string.IsNullOrWhiteSpace(storedToken.Value))
            {
                _identityLoaded = true;
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
                return;
            }

            var claims = ParseClaimsFromJwt(storedToken.Value);
            var expiry = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

            if (expiry is not null)
            {
                var expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
                if (expiryTime < DateTimeOffset.UtcNow.AddSeconds(30))
                {
                    // Token expired or about to expire - try refresh
                    var refreshed = await TryRefreshTokenAsync();
                    if (!refreshed)
                    {
                        await LogoutAsync();
                        return;
                    }
                    _identityLoaded = true;
                    return;
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            _currentUser = new ClaimsPrincipal(identity);
            _identityLoaded = true;

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gagal memuat identity dari storage.");
            _identityLoaded = true;
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }

    public async Task LoginAsync(string accessToken, string refreshToken)
    {
        await _localStorage.SetAsync(AccessTokenKey, accessToken);
        await _localStorage.SetAsync(RefreshTokenKey, refreshToken);

        var claims = ParseClaimsFromJwt(accessToken);
        var identity = new ClaimsIdentity(claims, "jwt");
        _currentUser = new ClaimsPrincipal(identity);
        _identityLoaded = true;

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task LogoutAsync()
    {
        await _localStorage.DeleteAsync(AccessTokenKey);
        await _localStorage.DeleteAsync(RefreshTokenKey);
        await _localStorage.DeleteAsync(UserInfoKey);

        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        _identityLoaded = true;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));

        // Redirect to login
        _navigationManager.NavigateTo("/Account/Login", true);
    }

    /// <summary>
    /// Called by the timer in MainLayout to proactively refresh tokens before expiry.
    /// </summary>
    public async Task RefreshTokensAsync()
    {
        if (_authenticationOptions.Provider == "None") return;

        try
        {
            var tokenResult = await _localStorage.GetAsync<string>(AccessTokenKey);
            if (!tokenResult.Success || string.IsNullOrWhiteSpace(tokenResult.Value))
                return;

            var claims = ParseClaimsFromJwt(tokenResult.Value);
            var expiry = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);

            if (expiry is null) return;

            var expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
            // Refresh if token expires within 5 minutes
            if (expiryTime > DateTimeOffset.UtcNow.AddMinutes(5))
                return;

            var refreshed = await TryRefreshTokenAsync();
            if (!refreshed)
            {
                _logger.LogWarning("Refresh token gagal. Melakukan logout otomatis.");
                await LogoutAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gagal memeriksa/refresh token.");
        }
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        try
        {
            var refreshResult = await _localStorage.GetAsync<string>(RefreshTokenKey);
            if (!refreshResult.Success || string.IsNullOrWhiteSpace(refreshResult.Value))
                return false;

            var request = new RefreshTokenRequest { RefreshToken = refreshResult.Value };
            var response = await _authService.RefreshTokenAsync(request);

            if (response.Error is not null || response.Result is null)
                return false;

            await _localStorage.SetAsync(AccessTokenKey, response.Result.AccessToken);
            await _localStorage.SetAsync(RefreshTokenKey, response.Result.RefreshToken);

            var newClaims = ParseClaimsFromJwt(response.Result.AccessToken);
            var identity = new ClaimsIdentity(newClaims, "jwt");
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
            _logger.LogInformation("Token berhasil di-refresh.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TryRefreshTokenAsync gagal.");
            return false;
        }
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
