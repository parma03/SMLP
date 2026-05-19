using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Features.Projects.Component;

public partial class CardProjectStatistic
{
    [Parameter]
    public string? Judul { get; set; }

    [Parameter]
    public string? Count { get; set; }

    [Parameter]
    public decimal Countdec { get; set; }

    [Parameter]
    public DateOnly? CountDate { get; set; }

    [Parameter]
    public string? Sub { get; set; }

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Rounded.People;

    [Parameter]
    public Color AvatarColor { get; set; } = Color.Tertiary;
}
