/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Psmp.Base.Selections.Enums;

namespace Psmp.Bsui.Features.Applicants.Selection.Models;

public class SelectionTimelineModel
{
    public SelectionStage SelectionStage { get; set; }
    public ApplicantStatus ApplicantStatus { get; set; }
    public string SelectionStageText { get; set; } = default!;
    public string ApplicantStatusText { get; set; } = default!;
    public string DotCss { get; set; } = default!;
    public string ClassCss { get; set; } = default!;
}
