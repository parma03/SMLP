using Microsoft.AspNetCore.Components;
using Smlp.Base.Selections.Enums;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Syncfusion.Blazor.DropDowns;

namespace Smlp.Bsui.Common.Components;

public partial class DropdownSelectionType
{
    [Parameter]
    public EventCallback<ChangeEventArgs<int, DropdownItemResponse>> OnChange { get; set; }

    protected List<DropdownItemResponse> _selectionType = new();
    public int SelectionTypeValue { get; set; } = 1;

    protected override void OnInitialized()
    {
        _selectionType = EnumExtentions.ToDropdownItem<SelectionType>();
        _selectionType = _selectionType.OrderByDescending(x => x.Id).ToList();
    }
}
