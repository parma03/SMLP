using Microsoft.AspNetCore.Components;
using Smlp.Shared.Common.Responses;
using Syncfusion.Blazor.DropDowns;

namespace Smlp.Bsui.Common.Components;

public partial class DropdownSelectionCode
{
    [Parameter]
    public EventCallback<ChangeEventArgs<int, DropdownItemResponse>> OnChangeAsync { get; set; }

    protected List<DropdownItemResponse> _codes = new();

    protected override async Task OnInitializedAsync()
    {
        _codes.Add(new DropdownItemResponse { Id = 0, Description = "All Code" });
        await Task.CompletedTask;
    }
}
