using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Psmp.Bsui.Common.Components;

public partial class CardStatistic
{
    [Parameter]
    public int Count { get; set; }

    [Parameter]
    public float CountFloat { get; set; }

    [Parameter]
    public decimal Countdec { get; set; }

    [Parameter]
    public string? Judul { get; set; }

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Rounded.People;

    [Parameter]
    public string TypeCount { get; set; } = "int";

    [Parameter]
    public string CountText { get; set; } = "";
    [Parameter]
    public bool IsLast { get; set; } = false;
}
