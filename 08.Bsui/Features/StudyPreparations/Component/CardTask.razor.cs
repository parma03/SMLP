using Microsoft.AspNetCore.Components;

namespace Smlp.Bsui.Features.StudyPreparations.Component;

public partial class CardTask
{
    [Parameter]
    public string? JudulTask { get; set; }
    [Parameter]
    public string? ValueTask { get; set; }
    [Parameter]
    public string? DateTask { get; set; }
}
