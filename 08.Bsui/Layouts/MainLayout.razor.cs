using System.Timers;
using Smlp.Bsui.Services.Geolocation.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Smlp.Base.ValueObjects;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Services.Authentication;
using Smlp.Bsui.Services.Telemetry;
using Smlp.Shared.Services.Authentication.Constants;
using Timer = System.Timers.Timer;

namespace Smlp.Bsui.Layouts;

public partial class MainLayout
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private readonly Timer _timerForRefreshTokens = new();

    private bool _firstTimeRender;
    private bool _drawerOpen;
    private bool _usingApplicationInsights;
    private bool _usingAuthentication;
    private bool _usingGeolocation;
    private GeolocationResult? _geolocationResult;
    private bool _readyToRenderBody;
    private bool _authorized;

    protected override async Task OnInitializedAsync()
    {
        _timerForRefreshTokens.Interval = TimeSpan.FromSeconds(_authenticationOptions.Value.RefreshRateInSeconds).TotalMilliseconds;
        _timerForRefreshTokens.Elapsed += RefreshTokens;
        _timerForRefreshTokens.AutoReset = false;

        _usingApplicationInsights = _telemetryOptions.Value.Provider is TelemetryProvider.ApplicationInsights;
        _usingAuthentication = _authenticationOptions.Value.Provider is not AuthenticationProvider.None;
        _usingGeolocation = _geolocationOptions.Value.Enabled;

        _firstTimeRender = true;
        _drawerOpen = true;

        await CheckAuthorize();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                var breakpoint = await _browserViewportService.GetCurrentBreakpointAsync();
                _drawerOpen = breakpoint is Breakpoint.Md or Breakpoint.Lg or Breakpoint.Xl or Breakpoint.Xxl;
            }
            catch
            {
                // Keep default drawer state if breakpoint detection is unavailable (e.g. during prerendering).
            }

            await ((AuthorizedAuthenticationStateProvider)_authenticationStateProvider).LoadIdentity();

            if (_usingGeolocation)
            {
                _geolocationResult = await _geolocationService.GetCurrentPosition();

                if (_geolocationResult.IsSuccess)
                {
                    var coordinates = _geolocationResult.Position.Coords;

                    _userInfo.Geolocation = new Geolocation(coordinates.Latitude, coordinates.Longitude, coordinates.Accuracy);
                }
            }

            _firstTimeRender = false;
            _timerForRefreshTokens.Start();

            await CheckAuthorize();

            StateHasChanged();
        }
    }

    private async Task CheckAuthorize()
    {
        if (_authenticationOptions.Value.AllowGuest)
        {
            _authorized = true;

            return;
        }

        var user = (await AuthenticationStateTask).User;

        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            _authorized = user.GetPermissions().Any();

            return;
        }

        _authorized = true;
    }

    private void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private void RefreshTokens(object? sender, ElapsedEventArgs e)
    {
        InvokeAsync(async () => await ((AuthorizedAuthenticationStateProvider)_authenticationStateProvider).RefreshTokensAsync());

        _timerForRefreshTokens.Stop();
        _timerForRefreshTokens.Start();
    }
}
