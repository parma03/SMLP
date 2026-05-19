using Microsoft.AspNetCore.Components;

namespace Smlp.Bsui.Features.Dashboards.Component;

public partial class CardStatistic
{
    [Parameter]
    public int Count { get; set; }
    [Parameter]
    public string? Judul { get; set; }
}
