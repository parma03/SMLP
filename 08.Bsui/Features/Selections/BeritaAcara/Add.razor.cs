using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Smlp.Shared.Common.Responses;

namespace Smlp.Bsui.Features.Selections.BeritaAcara;

public partial class Add
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    [Parameter]
    public string? Type { get; set; }
    [Parameter]
    public int Batch { get; set; }
    [Parameter]
    public int Year { get; set; }
}
