using Microsoft.AspNetCore.Components;
using Psmp.Base.Scholarships.Enums;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Common.Components;

public partial class DropdownSelectionCategory
{
    [Parameter]
    public EventCallback<ChangeEventArgs<int, DropdownItemResponse>> OnChange { get; set; }

    protected List<DropdownItemResponse> _categories = new();

    protected override void OnInitialized()
    {
        _categories = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
        _categories = _categories.OrderBy(x => x.Id == 0).ToList();
    }
}
