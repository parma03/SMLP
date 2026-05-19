/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Features.Applicants.Selection.Models;
using Psmp.Shared.Common.Extensions;

namespace Psmp.Bsui.Features.Applicants.Selection.Constants;

public static class TimelineItemToefl
{
    public static readonly SelectionTimelineModel InProgress = new()
    {
        SelectionStage = SelectionStage.TOEFL,
        ApplicantStatus = ApplicantStatus.InProgress,
        DotCss = TimelineStyleFor.InProgress,
        ClassCss = TimelineStyleFor.Intermediate,
        ApplicantStatusText = ApplicantStatus.InProgress.ToString().SplitWords(),
        SelectionStageText = SelectionStage.TOEFL.ToString(),
    };

    public static readonly SelectionTimelineModel Passed = new()
    {
        SelectionStage = SelectionStage.TOEFL,
        ApplicantStatus = ApplicantStatus.Passed,
        DotCss = TimelineStyleFor.Passed,
        ClassCss = TimelineStyleFor.Completed,
        ApplicantStatusText = ApplicantStatus.Passed.GetDescription(),
        SelectionStageText = SelectionStage.TOEFL.ToString(),
    };

    public static readonly SelectionTimelineModel Rejected = new()
    {
        SelectionStage = SelectionStage.TOEFL,
        ApplicantStatus = ApplicantStatus.Rejected,
        DotCss = TimelineStyleFor.Rejected,
        ClassCss = string.Empty,
        ApplicantStatusText = ApplicantStatus.Rejected.ToString(),
        SelectionStageText = SelectionStage.TOEFL.ToString(),
    };

    public static readonly SelectionTimelineModel Failed = new()
    {
        SelectionStage = SelectionStage.TOEFL,
        ApplicantStatus = ApplicantStatus.Failed,
        DotCss = TimelineStyleFor.Failed,
        ClassCss = string.Empty,
        ApplicantStatusText = ApplicantStatus.Failed.ToString(),
        SelectionStageText = SelectionStage.TOEFL.ToString(),
    };

    public static readonly SelectionTimelineModel Empty = new()
    {
        SelectionStage = SelectionStage.TOEFL,
        ApplicantStatus = ApplicantStatus.Failed,
        DotCss = string.Empty,
        ClassCss = string.Empty,
        ApplicantStatusText = string.Empty,
        SelectionStageText = SelectionStage.TOEFL.ToString(),
    };
}
