using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Smlp.Shared.Common.Responses;
//using Syncfusion.Blazor.Grids;
using Smlp.Shared.SiteAdministrator.Mentors.Queries;

namespace Smlp.Bsui.Features.SiteAdministrator.Mentor;
public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected List<GetMentorsResponse> _dataGrid = new();
    //private SfGrid<GetMentorsResponse> Grid { get; set; } = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;

    public FileUploadResponse FileUploadIeltsDoc { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity.IsAuthenticated)
        {
            await SetDataOnLoadAsync();
        }
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var response = await _mentorsService.GetMentorAsync();
        if (response.Error is null)
        {
            _dataGrid = response.Result!.Items.ToList();
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }
}
