using Microsoft.AspNetCore.Components;

namespace Smlp.Bsui.Common.Components;

public partial class DialogConfirmation
{
    [Parameter]
    public bool Visibility { get; set; }

    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public string Content { get; set; } = default!;

    [Parameter]
    public EventCallback DialogOpen { get; set; }

    [Parameter]
    public EventCallback DialogClose { get; set; }

    [Parameter]
    public EventCallback OnClickNo { get; set; }

    [Parameter]
    public EventCallback OnClickYes { get; set; }

    [Parameter]
    public bool IsProgressButton { get; set; } = false;

    [Parameter]
    public int Duration { get; set; } = 300;

    [Parameter]
    public string PositionX { get; set; } = "center";

    [Parameter]
    public string PositionY { get; set; } = "center";

    [Parameter] public bool UseLeftMargin { get; set; } = false;

}
