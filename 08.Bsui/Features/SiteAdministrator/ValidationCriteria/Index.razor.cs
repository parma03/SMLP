using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.AdministrativeCriterias.Commands;
using Psmp.Shared.Masters.AdministrativeCriterias.Queries;
using Microsoft.AspNetCore.Components.Web;
using Psmp.Bsui.Common.Extensions;

namespace Psmp.Bsui.Features.SiteAdministrator.ValidationCriteria;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    protected bool _isLoading = false;
    private bool VisibleSave { get; set; }

    protected List<DropdownItemResponse> _dropdownValidationCriteria = new();
    protected List<DropdownItemResponse> _dropdownUniversityLocation = new();
    protected List<DropdownItemResponse> _dropdownDegree = new();
    protected AdministrativeCriteriaRequest _administrativeCriteriaRequests = new();

    protected string _location;
    protected string _degree;

    protected List<GetAdministrativeCriteriaQueryResponse> _criteriaQueryResponses = new();
    protected int _formindex = 1;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        await InitDropdownValidationCriteria();
        await SetDataOnLoadAsync();
    }

    private async Task SetDataOnLoadAsync()
    {
        var request = new GetAdministrativeCriteriaQueryRequest()
        {
            Degree = _degree,
            Location = _location,
        };
        var response = await _administrativCriteriaService.GetAdministrativeCriteriaAsync(request);
        if (response.Error is null)
        {
            _criteriaQueryResponses = response.Result.Items.ToList();
        }

    }

    public async Task InitDropdownValidationCriteria()
    {
        _dropdownValidationCriteria = new()
        {
            new DropdownItemResponse {Id = 1, Description = "Administrative Selection" }
            //new DropdownItemResponse {Id = 2, Description = "Others" }
        };
        _dropdownUniversityLocation = new()
        {
            new DropdownItemResponse {Id = 1, Description = "Abroad" },
            new DropdownItemResponse {Id = 2, Description = "Domestic" }
        };
        await GetDropdownDegree();
    }

    private async Task GetDropdownDegree()
    {
        var data = await _degreeService.GetDegreesAsync();
        if (data.Error is null)
        {
            _dropdownDegree = data.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();
        }
    }

    public async Task OnChangeValidationCriteria(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _formindex = (int)args.ItemData.Id;
        StateHasChanged();
    }

    public async Task OnChangeUniversityLocation(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _location = args.ItemData.Description;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeDegree(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _degree = args.ItemData.Description;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    private async Task OnClickSave(MouseEventArgs e)
    {
        var updateData = _criteriaQueryResponses;
        string error = null;
        foreach (var item in updateData)
        {
            _administrativeCriteriaRequests.Valuse = item.Value;
            _administrativeCriteriaRequests.Id = item.Id;
            _administrativeCriteriaRequests.Name = item.Name;

            var response = await _administrativCriteriaService.UpdateAdministrativeCriteriaAsync(_administrativeCriteriaRequests);
            if (response.Error is not null)
            {
                error = response.Error.Detail;
            }
        }

        if (error is not null)
        {
            _snackbar.AddError(error);
        }
        else
        {
            _snackbar.AddSuccess($"Success updated Scholarship Administrativ Criteria");
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    private bool ValidateOnSave()
    {
        return true;
    }
}
