using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Psmp.Shared.Common.Responses;
using System.Security.Claims;

namespace Psmp.Bsui.Features.Selections.CreateSK;

public partial class Add
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    [Parameter]
    public Guid Id { get; set; }

    [Parameter]
    public int Batch { get; set; }

    [Parameter]
    public int Year { get; set; }
}
