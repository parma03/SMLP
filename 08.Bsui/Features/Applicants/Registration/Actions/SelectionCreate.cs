/*
*/

using Smlp.Client.Services.BackEnd.Selections;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Selections.SelectionAdministrations.Commands.Create;
using Smlp.Shared.Selections.SelectionOverviews.Commands.Create;
using Smlp.Shared.Selections.SelectionTOEFLs.Commands.Create;

namespace Smlp.Bsui.Features.Applicants.Registration.Actions;

public class SelectionCreate
{
    private readonly Registration _registration;
    private readonly SelectionOverviewService _overview;
    private readonly SelectionAdministrationService _administration;
    private readonly SelectionTOEFLService _toefl;

    public SelectionCreate(Registration registration, SelectionOverviewService overview, SelectionAdministrationService administration, SelectionTOEFLService toefl)
    {
        _registration = registration;
        _overview = overview;
        _administration = administration;
        _toefl = toefl;
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateSelectionOverview(Guid applicantId, bool isQualified)
    {
        var overview = new CreateSelectionOverviewRequest
        {
            ApplicantId = applicantId,
            Stage = Base.Selections.Enums.SelectionStage.Administration,
            Status = isQualified ? Base.Selections.Enums.ApplicantStatus.Passed : Base.Selections.Enums.ApplicantStatus.Failed
        };

        var response = await _overview.CreateSelectionOverviewAsync(overview);

        if (response.Error is null)
        {
            return response;
        }
        else
        {
            _registration.Error = response.Error;
            return new();
        }
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateSelectionAdministration(Guid applicantId, bool isQualified)
    {
        var administration = new CreateSelectionAdministrationRequest
        {
            ApplicantId = applicantId,
            Status = isQualified ? Base.Selections.Enums.ApplicantStatus.Passed : Base.Selections.Enums.ApplicantStatus.Failed
        };

        var response = await _administration.CreateSelectionAdministrationAsync(administration);

        if (response.Error is null)
        {
            return response;
        }
        else
        {
            _registration.Error = response.Error;
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
            _registration.Error = response.Error;
            return new();
        }
    }
}
