/*
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Base.Scholarships.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Services.Authentication.Constants;

namespace Smlp.Bsui.Common.Pages;

public partial class Profile
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    protected List<BreadcrumbItem> _breadcrumbItems = new();
    public ErrorResponse? Error { get; set; }

    protected bool _isLoading = false;
    protected string _displayName = default!;
    protected string _employeeNumber = default!;
    protected string _email = default!;
    protected string _company = default!;
    protected string _position = default!;
    protected DateOnly _birthday = DateOnly.MinValue;
    protected DateOnly _hiredate = DateOnly.MinValue;
    protected int _prl = 0;
    protected McuStatus _mcuStatus = McuStatus.No;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;

        _isLoading = true;

        _employeeNumber = _user.GetEmployeeId() ?? string.Empty;
        _email = _user.GetUsername() ?? string.Empty;
        _company = _user.GetCompanyCode() ?? string.Empty;
        _position = _user.GetPositionName() ?? string.Empty;

        await BypassUser();

        if (!string.IsNullOrEmpty(_employeeNumber))
        {
            await GetDataHris();
        }
        else
        {
            _snackbar.AddError(DefaultTextFor.IsEmptyFor(CommonDisplayTextFor.EmployeeNumber));
        }

        _isLoading = false;
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active(AuthenticationDisplayTextFor.Profile)
        };
    }
    private async Task BypassUser()
    {
        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeNumber = asResponse.Result.EmployeeIdAlias;
                _email = asResponse.Result.EmailAlias;
            }
        }
    }

    private async Task GetDataHris()
    {
        var profile = await _hrisService.GetEmployeeByIdAsync(_employeeNumber);
        if (profile.EmployeeId != null)
        {
            _company = $"{profile.CompanyCode} - {profile.CompanyName}";
            _position = profile.PositionName;

            var birthDate = profile.BirthDate ?? DateTime.MinValue.ToString("MM/dd/yyyy");
            _birthday = DateOnly.FromDateTime(DateTimeExtensions.ToDateTime(birthDate));

            var hiringDate = profile.HiringDate ?? DateTime.MinValue.ToString("MM/dd/yyyy");
            _hiredate = DateOnly.FromDateTime(DateTimeExtensions.ToDateTime(hiringDate));

            if (!string.IsNullOrEmpty(profile.McuResult))
            {
                _mcuStatus = Enum.Parse<McuStatus>(profile.McuResult);
            }

            if (!string.IsNullOrEmpty(profile.Prl))
            {
                var prl = profile.Prl ?? "0";
                _prl = Convert.ToInt16(prl);
            }
            else if (!string.IsNullOrEmpty(profile.HomePrl))
            {
                var prl = profile.HomePrl ?? "0";
                _prl = Convert.ToInt16(prl);
            }
        }
    }
}
