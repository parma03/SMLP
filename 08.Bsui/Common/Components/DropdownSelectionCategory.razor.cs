using Microsoft.AspNetCore.Components;
using Smlp.Base.Scholarships.Enums;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Syncfusion.Blazor.DropDowns;

namespace Smlp.Bsui.Common.Components;

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
