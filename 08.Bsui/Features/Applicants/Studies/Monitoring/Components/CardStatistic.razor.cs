/*
*/

using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Features.Applicants.Studies.Monitoring.Components;

public partial class CardStatistic
{
    [Parameter]
    public float Count { get; set; } = 0;

    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public string? SubTitle { get; set; } = default!;

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Rounded.Summarize;

    [Parameter]
    public Color AvatarColor { get; set; } = Color.Tertiary;
}
