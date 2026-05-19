using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Psmp.Shared.Common.Responses;
using System.Security.Claims;
using Psmp.Shared.SiteAdministrators.Buddy.Queries;

namespace Psmp.Bsui.Features.SiteAdministrator.Buddy;
public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;

    protected List<GetBuddyResponse> _data = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        await SetDataOnLoadAsync();
    }
    private async Task SetDataOnLoadAsync()
    {
        var response = await _buddyService.GetBuddyAsync();
        if (response.Error is null)
        {
            _data = response.Result!.Items.ToList();
        }

    }
}
