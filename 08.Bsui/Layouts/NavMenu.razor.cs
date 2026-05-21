using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Smlp.Bsui.Layouts;

public partial class NavMenu
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
    }
}
