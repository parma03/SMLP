using Microsoft.AspNetCore.Components;

namespace Psmp.Bsui.Features.Selections.Applicant;

public partial class ScheduleDetail
{
    [Parameter]
    public bool Visibility { get; set; }

    [Parameter]
    public EventCallback DialogOpen { get; set; }

    [Parameter]
    public EventCallback DialogClose { get; set; }

    [Parameter]
    public EventCallback OnClickNo { get; set; }

    [Parameter]
    public EventCallback OnClickSave { get; set; }
}
