/*
*/

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Applicants.Queries.GetApplicantSchedule;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Queries.GetDocumentFile;

namespace Smlp.Bsui.Features.Applicants.Selection.Components;

public partial class DialogSelectionStep
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public Guid ApplicantId { get; set; }

    [Parameter]
    public Guid ScholarshipId { get; set; }

    [Parameter]
    public Base.Selections.Enums.SelectionStage SelectionStage { get; set; }

    [Parameter]
    public string SelectionStageText { get; set; } = string.Empty;

    public ErrorResponse? Error { get; set; }
    protected string _scheduleTest = string.Empty;
    protected string _scheduleTime = string.Empty;
    protected string _scheduleLocation = string.Empty;
    protected string _testResult = string.Empty;
    protected string _testScore = string.Empty;
    protected string _testDocument = string.Empty;
    protected Guid _documentId = Guid.Empty;
    protected Guid _documentSkId = Guid.Empty;
    protected Guid _documentSpId = Guid.Empty;

    protected override async Task OnInitializedAsync()
    {
        var request = new GetApplicantScheduleRequest
        {
            ScholarshipId = ScholarshipId,
            ApplicantId = ApplicantId
        };

        if (SelectionStage == SelectionStage.TOEFL)
        {
            await SetDataToefl(request);
        }
        else if (SelectionStage == SelectionStage.Psychotest)
        {
            await SetDataPsychotest(request);
        }
        else if (SelectionStage is SelectionStage.TalentReview or SelectionStage.TalentReview_2 or SelectionStage.Decree)
        {
            await SetDataDecree(request);
        }
        else
        {
            _snackbar.AddWarning(NotificationMessageFor.NotInSelectionStage);
        }
    }

    private async Task SetDataToefl(GetApplicantScheduleRequest request)
    {
        var schedule = await _scheduleService.GetApplicantToeflScheduleAsync(request);
        if (schedule.Error == null && schedule.Result != null)
        {
            var result = schedule.Result;
            _scheduleTest = result.ScheduleStart.ToString("dd MMM yyyy");
            _scheduleTime = result.SessionStart.ToString("c", CultureInfo.InvariantCulture);
            _scheduleLocation = result.Location;

            var toefl = await _selectionTOEFLService.GetSelectionToeflByApplicantIdAsync(ApplicantId);
            if (toefl.Error == null && toefl.Result != null)
            {
                if (result.ScheduleStart == DateOnly.MinValue)
                {
                    _scheduleTest = toefl.Result.TestDate?.ToString("dd MMM yyyy") ?? string.Empty;
                    _scheduleTime = DefaultTextFor.Dash;
                    _scheduleLocation = DefaultTextFor.Dash;
                }

                _testResult = toefl.Result.Status.GetDescription();
                _testScore = toefl.Result.Score.ToString();
                _documentId = toefl.Result.Document != null ? toefl.Result.Document.Id : Guid.Empty;
            }
        }
        else
        {
            Error = schedule.Error;
            StateHasChanged();
        }
    }

    private async Task SetDataPsychotest(GetApplicantScheduleRequest request)
    {
        var schedule = await _scheduleService.GetApplicantPsychotestScheduleAsync(request);
        if (schedule.Error == null && schedule.Result != null)
        {
            var result = schedule.Result;
            _scheduleTest = result.ScheduleStart.ToString("dd MMM yyyy");
            _scheduleTime = result.SessionStart.ToString("c", CultureInfo.InvariantCulture);
            _scheduleLocation = result.Location;

            var psychotest = await _selectionPsychotestService.GetSelectionPsychotestByApplicantIdAsync(ApplicantId);
            if (psychotest.Error == null && psychotest.Result != null)
            {
                if (result.ScheduleStart == DateOnly.MinValue)
                {
                    _scheduleTest = psychotest.Result.TestDate?.ToString("dd MMM yyyy") ?? string.Empty;
                    _scheduleTime = DefaultTextFor.Dash;
                    _scheduleLocation = DefaultTextFor.Dash;
                }

                _testResult = psychotest.Result.Status.ToString();
                _testScore = psychotest.Result.QualificationStatus.ToString();
                _documentId = psychotest.Result.Document != null ? psychotest.Result.Document.Id : Guid.Empty;
            }
        }
        else
        {
            Error = schedule.Error;
            StateHasChanged();
        }
    }

    private async Task SetDataDecree(GetApplicantScheduleRequest request)
    {
        var res = await _selectionDecreesService.GetSelectionDecreeDocumentByIdAsync(request.ScholarshipId);
        if (res.Error == null && res.Result != null)
        {
            var result = res.Result;
            _documentSkId = result.DecreeDocumentId ?? Guid.Empty;
            _documentSpId = result.WarrantDocumentId ?? Guid.Empty;
        }
        else
        {
            Error = res.Error;
            StateHasChanged();
        }
    }

    protected void OnClickClose()
    {
        MudDialog.Close(DialogResult.Ok(false));
    }

    protected async Task OnClickDownloadCertificate()
    {
        var request = new GetDocumentFileRequest();
        if (SelectionStage == SelectionStage.TOEFL)
        {
            request = new GetDocumentFileRequest
            {
                DocumentId = _documentId,
                DocumentType = DocumentType.CertificateTOEFL,
            };
        }
        else
        {
            request = new GetDocumentFileRequest
            {
                DocumentId = _documentId,
                DocumentType = DocumentType.CertificatePsychotest,
            };
        }

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            Error = response.Error;
            StateHasChanged();
            return;
        }
        else if (response.Result is null)
        {
            _snackbar.AddError($"Failed to retrieve the document {CommonDisplayTextFor.File}.");
            return;
        }
        else
        {
            await _jsRuntime!.InvokeVoidAsync(
            JavaScriptIdentifierFor.DownloadFile,
            response.Result.FileName,
            response.Result.ContentType,
            response.Result.Content);
        }
    }

    protected async Task OnClickDownloadSK()
    {
        var request = new GetDocumentFileRequest();
        request = new GetDocumentFileRequest
        {
            DocumentId = _documentSkId,
            DocumentType = DocumentType.ScholarshipDecree,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            Error = response.Error;
            StateHasChanged();
            return;
        }
        else if (response.Result is null)
        {
            _snackbar.AddError($"Failed to retrieve the document {CommonDisplayTextFor.File}.");
            return;
        }
        else
        {
            await _jsRuntime!.InvokeVoidAsync(
            JavaScriptIdentifierFor.DownloadFile,
            response.Result.FileName,
            response.Result.ContentType,
            response.Result.Content);
        }
    }

    protected async Task OnClickDownloadSP()
    {
        var request = new GetDocumentFileRequest();
        request = new GetDocumentFileRequest
        {
            DocumentId = _documentSpId,
            DocumentType = DocumentType.ScholarshipWarrant,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            Error = response.Error;
            StateHasChanged();
            return;
        }
        else if (response.Result is null)
        {
            _snackbar.AddError($"Failed to retrieve the document {CommonDisplayTextFor.File}.");
            return;
        }
        else
        {
            await _jsRuntime!.InvokeVoidAsync(
            JavaScriptIdentifierFor.DownloadFile,
            response.Result.FileName,
            response.Result.ContentType,
            response.Result.Content);
        }
    }
}
