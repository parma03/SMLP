/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Services.External.Location.Models;
using Psmp.Shared.Common.Constants;
using Psmp.Bsui.Common.Extensions;

namespace Psmp.Bsui.Common.Pages;

public partial class About
{
    private const string Cookie = nameof(Cookie);

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private GeolocationDetails _userGeolocationDetails = default!;

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsAdmin { get; set; } = false;
    public bool IsMentor { get; set; } = false;

    private readonly Dictionary<string, string[]> _requestHeaders = new();
    private readonly Dictionary<string, string[]> _responseHeaders = new();
    private ConnectionInfo _connectionInfo = default!;
    private string _rootpath;
    protected string _employeeId = string.Empty;
    private bool _isHC = false;

    protected override async Task OnInitializedAsync()
    {
        //var permissions = new List<string>
        //{
        //    "scholarship", "Selection", "Payment"
        //};
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity!.IsAuthenticated)
        {
            var permissionClaims = _user.Claims
                .Where(x => x.Type.Equals("permission", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value.ToLower().Trim())
                .ToList();

            if (permissionClaims.Any(p => p.Contains("selection") || p.Contains("payment")))
            {
                IsAdmin = true;
            }
            else
            {
                var mentorResponse = await _applicantService.GetMentorByEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
                if (mentorResponse.Error is null && mentorResponse.Result is not null)
                {
                    IsMentor = true;
                }

                if (permissionClaims.Any(p => p.Contains("scholarship")))
                {
                    _isHC = true;
                }
            }
        }

        _employeeId = _user.GetEmployeeId() ?? string.Empty;

        #region bypass user applicant

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;
            }
        }

        #endregion bypass user applicant

        SetupBreadcrumb();

        _rootpath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot\\file\\template");

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            _connectionInfo = httpContext.Connection;

            foreach (var header in httpContext.Request.Headers)
            {
                if (!header.Key.Equals("Cookie"))
                {
                    _requestHeaders.Add(header.Key, header.Value.ToArray());
                }
            }

            foreach (var header in httpContext.Response.Headers)
            {
                _responseHeaders.Add(header.Key, header.Value.ToArray());
            }
        }

        if (_geolocationOptions.Value.Enabled)
        {
            if (_userInfo.Geolocation is not null)
            {
                _userGeolocationDetails = await _locationExternalService.GetGeolocationDetails(_userInfo.Geolocation);

                StateHasChanged();
            }
        }

        var mentor = await _applicantService.GetMentorByEmployeeIdAsync(_employeeId);
        if (mentor.Error is null)
        {
            if (mentor.Result is not null)
            {
                IsMentor = true;
            }
        }
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active(CommonDisplayTextFor.About)
        };
    }
}
