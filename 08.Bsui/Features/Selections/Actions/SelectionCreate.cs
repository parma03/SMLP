using Smlp.Base.Selections.Enums;
using Smlp.Client.Services.BackEnd.Selections;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Selections.SelectionAdministrations.Commands.Create;
using Smlp.Shared.Selections.SelectionOverviews.Commands.Create;
using Smlp.Shared.Selections.SelectionPsychotests.Commands.Create;
using Smlp.Shared.Selections.SelectionPsychotests.Commands.Update;
using Smlp.Shared.Selections.SelectionTalentReviewFirsts.Commands.Create;
using Smlp.Shared.Selections.SelectionTOEFLs.Commands.Create;

namespace Smlp.Bsui.Features.Selections.Actions;

public class SelectionCreate
{
    private readonly SelectionOverviewService _overview;
    private readonly SelectionAdministrationService _administration;
    private readonly SelectionTOEFLService _toefl;
    private readonly SelectionPsychotestService _psychotest;
    private readonly SelectionTalentReviewFirstService _talentReviewFirst;

    public SelectionCreate(SelectionOverviewService overview, SelectionAdministrationService administration, SelectionTOEFLService toefl, SelectionPsychotestService psychotest, SelectionTalentReviewFirstService talentReviewFirstService)
    {
        _overview = overview;
        _administration = administration;
        _toefl = toefl;
        _psychotest = psychotest;
        _talentReviewFirst = talentReviewFirstService;
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateSelectionOverview(Guid applicantId, string? type)
    {
        var overview = new CreateSelectionOverviewRequest
        {
            ApplicantId = applicantId,
            Stage = type == null ? Base.Selections.Enums.SelectionStage.Administration : SelectionStage.TalentReview_1,
            Status = Base.Selections.Enums.ApplicantStatus.Passed
        };

        var response = await _overview.CreateSelectionOverviewAsync(overview);

        if (response.Error is null)
        {
            return response;
        }
        else
        {
            //_selection._error = response.Error;
            return new();
        }
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateSelectionAdministration(Guid applicantId)
    {
        var administration = new CreateSelectionAdministrationRequest
        {
            ApplicantId = applicantId,
            Status = Base.Selections.Enums.ApplicantStatus.Passed
        };

        var response = await _administration.CreateSelectionAdministrationAsync(administration);

        if (response.Error is null)
        {
            return response;
        }
        else
        {
            //_selection._error = response.Error;
            return new();
        }
    }
    public async Task<ResponseResult<CreateSelectionTalentReviewFirstsResponse>> CreateSelectionTalentReviewFirst(Guid applicantId)
    {
        var selectionTalentReviewFirst = new CreateSelectionTalentReviewFirstsRequest
        {
            ApplicantId = applicantId,
            Status = Base.Selections.Enums.ApplicantStatus.Passed
        };
        var response = await _talentReviewFirst.CreateSelectionTalentReviewFirstAsync(selectionTalentReviewFirst);
        if (response.Error is null)
        {
            return response;
        }
        else
        {
            //_selection._error = response.Error;
            return new();
        }
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateSelectionToefl(Guid applicantId)
    {
        var toefl = new CreateSelectionTOEFLRequest
        {
            ApplicantId = applicantId,
            Status = Base.Selections.Enums.ApplicantStatus.InProgress,
            Score = 0
        };

        var response = await _toefl.CreateSelectionTOEFLAsync(toefl);

        if (response.Error is null)
        {
            return response;
        }
        else
        {
            //_selection._error = response.Error;
            return new();
        }
    }
    public async Task<ResponseResult<CommonCreateResponse>> CreateSelectionPsychotest(Guid applicantId)
    {
        var psychotest = new CreateSelectionPsychotestRequest
        {
            ApplicantId = applicantId,
            Status = Base.Selections.Enums.ApplicantStatus.InProgress,
            QualificationStatus = SelectionQualificationStatus.NotQualified,
        };

        var response = await _psychotest.CreateSelectionPsychotestAsync(psychotest);

        if (response.Error is null)
        {
            return response;
        }
        else
        {
            //_selection._error = response.Error;
            return new();
        }
    }
    #region Method update
    public async Task<ResponseResult<CommonUpdateResponse>> UpdateSelectionPsychotest(Guid applicantId, ApplicantStatus applicantStatus)
    {
        var psychotest = new UpdateSelectionPsychotestRequest
        {
            Id = applicantId,
            Status = applicantStatus,
        };

        var response = await _psychotest.UpdateSelectionPsychotestAsync(psychotest);

        if (response.Error is null)
        {
            return response;
        }
        else
        {
            //_selection._error = response.Error;
            return new();
        }
    }
    #endregion
}
