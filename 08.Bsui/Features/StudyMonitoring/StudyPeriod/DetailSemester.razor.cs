using Microsoft.AspNetCore.Components;
using MudBlazor;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.StudyMonitorings.Constants;
using Psmp.Shared.StudyMonitorings.StudyPeriod.Queries;

namespace Psmp.Bsui.Features.StudyMonitoring.StudyPeriod;

public partial class DetailSemester
{
    #region Parameter
    [Parameter]
    public Guid AdmissionId { get; set; }

    [Parameter]
    public Guid Id { get; set; }
    #endregion

    #region Protected
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    protected GetStudyPeriodByIdQueryResponse _data = new();
    #endregion

    #region Document
    protected string? _documentPassport;
    protected string? _documentVisa;
    protected string? _documentTranscriptFile;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        await OnLoadPage();
        await SetDataOnLoadAsync();

        _isLoading = false;
        await Task.CompletedTask;
    }

    private async Task OnLoadPage()
    {
        _breadcrumbItems.Clear();
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPeriod));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Detail));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Semester));

        await Task.CompletedTask;
    }

    #region Data
    private async Task SetDataOnLoadAsync()
    {
        var dataAdmission = await _studyMonitoringStudyPeriodService.GetStudyPeriodByAdmissionIdAsync(AdmissionId);
        if (dataAdmission.Result == null || dataAdmission.Result!.Items.Count == 0)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringTab(0), true);
            StateHasChanged();
            return;
        }

        var data = await _studyMonitoringStudyPeriodService.GetStudyPeriodByIdAsync(Id);
        if (data.Result == null || data.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringDetail("StudyPeriod", AdmissionId), true);
            StateHasChanged();
            return;
        }

        if (data.Error is null)
        {
            _data = data.Result;
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.PassportDocumentId, DocumentType = DocumentType.Passport });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentPassport = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }

            resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.VisaDocumentId, DocumentType = DocumentType.Visa });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentVisa = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }

            resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.TranscriptDocumentId, DocumentType = DocumentType.Transcript });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentTranscriptFile = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
        else
        {
            _error = data.Error;
        }

        await Task.CompletedTask;
    }
    #endregion

    #region OnClick
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringDetail("StudyPeriod", AdmissionId), true);
        await Task.CompletedTask;
    }
    #endregion
}
