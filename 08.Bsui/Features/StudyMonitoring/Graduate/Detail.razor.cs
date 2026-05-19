using Microsoft.AspNetCore.Components;
using MudBlazor;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.StudyMonitorings.Constants;
using Psmp.Shared.StudyMonitorings.Graduate.Queries;

namespace Psmp.Bsui.Features.StudyMonitoring.Graduate;

public partial class Detail
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }
    #endregion

    #region Protected
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected GetGraduateByIdQueryResponse _data = new();
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    #endregion

    #region Document
    protected string? _documentLastExamp;
    protected string? _documentGraduation;
    protected string? _documentTranscriptFile;
    protected string? _documentCompletetion;
    protected string? _documentCourseFinal;
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
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Graduate));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Detail));

        await Task.CompletedTask;
    }

    #region DataGrid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _studyMonitoringGraduateService.GetGraduateByIdAsync(Id);
        if (data.Result == null || data.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringTab(1), true);
            StateHasChanged();
            return;
        }

        if (data.Error is null)
        {
            _data = data.Result;

            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.LastExamDocumentId ?? Guid.Empty, DocumentType = DocumentType.LastExam });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentLastExamp = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }

            resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.GraduationDocumentId ?? Guid.Empty, DocumentType = DocumentType.Graduation });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentGraduation = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }

            resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.TranscriptDocumentId ?? Guid.Empty, DocumentType = DocumentType.Transcript });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentTranscriptFile = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }

            resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.CompletionDocumentId ?? Guid.Empty, DocumentType = DocumentType.Completion });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentCompletetion = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }

            resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.CourseFinalDocumentId ?? Guid.Empty, DocumentType = DocumentType.FinalProject });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _documentCourseFinal = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
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
        _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringTab(1), true);
        await Task.CompletedTask;
    }
    #endregion
}
