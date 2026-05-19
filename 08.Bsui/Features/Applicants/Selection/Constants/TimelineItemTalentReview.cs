/*
*/

using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Features.Applicants.Selection.Models;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Bsui.Features.Applicants.Selection.Constants;

public static class TimelineItemTalentReview
{
    public static readonly SelectionTimelineModel InProgress = new()
    {
        SelectionStage = SelectionStage.TalentReview,
        ApplicantStatus = ApplicantStatus.InProgress,
        DotCss = TimelineStyleFor.InProgress,
        ClassCss = TimelineStyleFor.Intermediate,
        ApplicantStatusText = ApplicantStatus.InProgress.ToString().SplitWords(),
        SelectionStageText = SelectionStage.TalentReview.ToString().SplitWords(),
    };

    public static readonly SelectionTimelineModel Failed = new()
    {
        SelectionStage = SelectionStage.TalentReview,
        ApplicantStatus = ApplicantStatus.Failed,
        DotCss = TimelineStyleFor.Failed,
        ClassCss = TimelineStyleFor.Intermediate,
        ApplicantStatusText = ApplicantStatus.Failed.GetDescription(),
        SelectionStageText = SelectionStage.TalentReview.ToString().SplitWords(),
    };

    public static readonly SelectionTimelineModel Passed = new()
    {
        SelectionStage = SelectionStage.TalentReview,
        ApplicantStatus = ApplicantStatus.Passed,
        DotCss = TimelineStyleFor.Passed,
        ClassCss = TimelineStyleFor.Completed,
        ApplicantStatusText = ApplicantStatus.Passed.GetDescription(),
        SelectionStageText = SelectionStage.TalentReview.ToString().SplitWords(),
    };

    public static readonly SelectionTimelineModel Rejected = new()
    {
        SelectionStage = SelectionStage.TalentReview,
        ApplicantStatus = ApplicantStatus.Rejected,
        DotCss = TimelineStyleFor.Rejected,
        ClassCss = string.Empty,
        ApplicantStatusText = ApplicantStatus.Rejected.ToString(),
        SelectionStageText = SelectionStage.TalentReview.ToString().SplitWords(),
    };

    public static readonly SelectionTimelineModel Empty = new()
    {
        SelectionStage = SelectionStage.TalentReview,
        ApplicantStatus = ApplicantStatus.Failed,
        DotCss = string.Empty,
        ClassCss = string.Empty,
        ApplicantStatusText = string.Empty,
        SelectionStageText = SelectionStage.TalentReview.ToString().SplitWords(),
    };
}
