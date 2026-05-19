/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Features.Applicants.Selection.Models;
using Psmp.Shared.Common.Extensions;

namespace Psmp.Bsui.Features.Applicants.Selection.Constants;

public static class TimelineItemTalentReview2nd
{
    public static readonly SelectionTimelineModel InProgress = new()
    {
        SelectionStage = SelectionStage.TalentReview_2,
        ApplicantStatus = ApplicantStatus.InProgress,
        DotCss = TimelineStyleFor.InProgress,
        ClassCss = TimelineStyleFor.Intermediate,
        ApplicantStatusText = ApplicantStatus.InProgress.ToString().SplitWords(),
        SelectionStageText = "Talent Review 2",
    };

    public static readonly SelectionTimelineModel Passed = new()
    {
        SelectionStage = SelectionStage.TalentReview_2,
        ApplicantStatus = ApplicantStatus.Passed,
        DotCss = TimelineStyleFor.Passed,
        ClassCss = TimelineStyleFor.Completed,
        ApplicantStatusText = ApplicantStatus.Passed.GetDescription(),
        SelectionStageText = "Talent Review 2",
    };

    public static readonly SelectionTimelineModel Rejected = new()
    {
        SelectionStage = SelectionStage.TalentReview_2,
        ApplicantStatus = ApplicantStatus.Rejected,
        DotCss = TimelineStyleFor.Rejected,
        ClassCss = string.Empty,
        ApplicantStatusText = ApplicantStatus.Rejected.ToString(),
        SelectionStageText = "Talent Review 2",
    };

    public static readonly SelectionTimelineModel Empty = new()
    {
        SelectionStage = SelectionStage.TalentReview_2,
        ApplicantStatus = ApplicantStatus.Failed,
        DotCss = string.Empty,
        ClassCss = string.Empty,
        ApplicantStatusText = string.Empty,
        SelectionStageText = "Talent Review 2",
    };
}
