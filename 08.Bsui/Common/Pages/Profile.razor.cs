using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Common.Constants;

namespace Smlp.Bsui.Common.Pages;

public partial class Profile
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private string _displayName = string.Empty;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _role = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var user = (await AuthenticationStateTask).User;
        if (user.Identity?.IsAuthenticated == true)
        {
            _displayName = user.Identity.Name ?? "Pengguna";
            _username = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            _email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            _role = user.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active(CommonDisplayTextFor.Profile)
        };
    }
}
