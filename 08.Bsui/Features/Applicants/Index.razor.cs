/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Extensions;

namespace Psmp.Bsui.Features.Applicants;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    protected string _companyName = default!;
    protected string _greetings = default!;
    protected bool _isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _user = (await AuthenticationStateTask).User;

        _greetings = $"Good {DateTimeOffset.Now.ToFriendlyTimeDisplayText()}";
        var company = await _companyService.GetCompanyByCodeAsync(_user.GetCompanyCode() ?? string.Empty);
        if (company.Error is null)
        {
            _companyName = company.Result!.Name ?? string.Empty;
        }

        _isLoading = false;
    }
}
