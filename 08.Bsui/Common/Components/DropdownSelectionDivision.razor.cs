using Microsoft.AspNetCore.Components;
using Smlp.Shared.Masters.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Smlp.Bsui.Common.Components;

public partial class DropdownSelectionDivision
{
    [Parameter]
    public EventCallback<ChangeEventArgs<int, GetDivisionQueryResponse>> OnChangeAsync { get; set; }

    protected List<GetDivisionQueryResponse> _divisions = new();

    protected override async Task OnInitializedAsync()
    {
        _divisions.Add(new GetDivisionQueryResponse { Id = 0, Name = "All Division" });
        await Task.CompletedTask;
    }
}
