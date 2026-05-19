using Microsoft.AspNetCore.Components;
using Psmp.Shared.Masters.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Common.Components;

public partial class DropdownSelectionSubHolding
{
    [Parameter]
    public EventCallback<ChangeEventArgs<int, GetCompanyGroupQueryResponse>> OnChangeAsync { get; set; }

    protected List<GetCompanyGroupQueryResponse> _companiesGroup = new();
    protected int CompanyGroupValue { get; set; } = 0;

    protected override async Task OnInitializedAsync()
    {
        _companiesGroup.Add(new GetCompanyGroupQueryResponse { Id = 0, Name = "All H/SH/AP&P" });
        await Task.CompletedTask;
    }
}
