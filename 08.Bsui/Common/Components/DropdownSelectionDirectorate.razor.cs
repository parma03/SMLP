using Microsoft.AspNetCore.Components;
using Psmp.Shared.Masters.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Common.Components;

public partial class DropdownSelectionDirectorate
{
    [Parameter]
    public EventCallback<ChangeEventArgs<int, GetDirectorateQueryResponse>> OnChangeAsync { get; set; }

    protected int DirectorateValue { get; set; } = 0;
    protected List<GetDirectorateQueryResponse> _directorates = new();

    protected override async Task OnInitializedAsync()
    {
        _directorates.Add(new GetDirectorateQueryResponse { Id = 0, Name = "All Directorate" });
        await Task.CompletedTask;
    }
}
