using Microsoft.AspNetCore.Components;
using Psmp.Shared.Masters.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Common.Components;

public partial class DropdownCompanyGroup
{
    [Parameter]
    public EventCallback<ChangeEventArgs<int, GetCompanyGroupQueryResponse>> OnChangeAsync { get; set; }

    protected int CompanyGrupValue { get; set; }
    protected List<GetCompanyGroupQueryResponse> _companyGroups = new();

    protected override async Task OnInitializedAsync()
    {
        var companiesGroup = await _companyGroupService.GetCompanyGroupsAsync();
        if (companiesGroup.Error is null)
        {
            _companyGroups = companiesGroup.Result!.Items.ToList();
            _companyGroups.Add(new GetCompanyGroupQueryResponse { Id = 9999, Name = "All" });

            _companyGroups = _companyGroups.OrderBy(x => x.Name).ToList();
            CompanyGrupValue = _companyGroups.FirstOrDefault(x => x.Id == 9999)!.Id;
        }
    }
}
