using Microsoft.AspNetCore.Components;

namespace Smlp.Bsui.Features.StudyPreparations.Component;

public partial class CardStatistic
{
    [Parameter]
    public int Count { get; set; }
    [Parameter]
    public decimal Countdec { get; set; }
    [Parameter]
    public string? CountString { get; set; }
    [Parameter]
    public string? Judul { get; set; }
    [Parameter]
    public bool IsLast { get; set; }
}
