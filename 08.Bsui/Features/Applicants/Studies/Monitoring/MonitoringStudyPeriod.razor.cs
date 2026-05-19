/*
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.StudyMonitorings.StudyPeriod.Commands.Create;
using Smlp.Shared.StudyMonitorings.StudyPeriod.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Smlp.Bsui.Features.Applicants.Studies.Monitoring;

public partial class MonitoringStudyPeriod
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    private bool IsEnabled { get; set; }
    private bool VisiblePopup { get; set; }
    public ErrorResponse? Error { get; set; }
    protected Guid ApplicantId { get; set; }
    protected Guid AdmissionId { get; set; }
    protected Guid DocumentTranscriptId { get; set; }
    protected FileUploadResponse FileUploadTranscript { get; set; } = new();

    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string _displayName = default!;
    protected string _universityName = default!;
    protected float _gpaMaximum = 0;
    protected float _gpaMinimum = 0;
    protected float _gpaAverage = 0;
    protected CreateStudyPeriodRequest _requestStudyPeriod = new();
    protected IList<GetStudyPeriodByAdmissionIdQueryResponse> _dataGrid = default!;
    protected long _maximumFileSizeInBytes = long.MaxValue;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _universityName = string.Empty;
        _isLoading = true;

        #region bypass user applicant

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;
                _username = asResponse.Result.EmailAlias;
            }
        }

        if (string.IsNullOrEmpty(_employeeId))
        {
            _snackbar.AddError("Not Found Employee ID");
            _isLoading = false;
            return;
        }

        #endregion bypass user applicant

        //get scholarship existing
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error != null)
        {
            Error = applicant.Error;
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            ApplicantId = lastApplicant!.Id;

            var admissions = await _universityAdmissionService.GetUniversityAdmissionAsync();
            if (admissions.Error != null)
            {
                Error = admissions.Error;
            }
            else
            {
                var admission = admissions.Result!.Items.Where(x => x.ApplicantId == ApplicantId);
                if (!admission.Any())
                {
                    _snackbar.AddWarning("Applicant not yet input Admission");
                }
                else
                {
                    _universityName = admission.FirstOrDefault()!.University;
                    AdmissionId = admission.FirstOrDefault()!.Id;

                    await SetGridStudyPeriod();
                }
            }
        }

        _isLoading = false;
    }

    private async Task SetGridStudyPeriod()
    {
        var monitoring = await _studyMonitoringStudyPeriodService.GetStudyPeriodByAdmissionIdAsync(AdmissionId);
        if (monitoring.Error != null)
        {
            Error = monitoring.Error;
        }
        else
        {
            _dataGrid = new List<GetStudyPeriodByAdmissionIdQueryResponse>();
            _dataGrid = monitoring.Result!.Items.OrderByDescending(x => x.Semester).ToList();

            if (monitoring.Result!.Items.Count() > 0)
            {
                _gpaMaximum = monitoring.Result!.Items.Max(x => x.GpaScore);
                _gpaMinimum = monitoring.Result!.Items.Min(x => x.GpaScore);

                var count = _dataGrid.Count;
                var sum = _dataGrid.Sum(x => x.GpaScore);
                _gpaAverage = sum / count;
            }
        }
    }

    private void OnClickAddAsync()
    {
        DialogOpen();
    }

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        _requestStudyPeriod = new CreateStudyPeriodRequest();
        VisiblePopup = false;
    }

    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        #region upload transcript

        if (FileUploadTranscript.FileName is null)
        {
            _snackbar.AddWarning("Please browse Transcript !");
            return;
        }

        await UploadTranscript(_employeeId);

        if (DocumentTranscriptId == Guid.Empty)
        {
            _snackbar.AddWarning("Please upload Transcript !");
            return;
        }

        #endregion upload transcript

        #region insert study preparation

        _requestStudyPeriod.AdmissionId = AdmissionId;
        _requestStudyPeriod.TranscriptDocumentId = DocumentTranscriptId;

        _isLoadingDialog = true;
        StateHasChanged(); // update tampilan agar spinner muncul

        var response = await _studyMonitoringStudyPeriodService.CreateStudyPeriodAsync(_requestStudyPeriod);
        if (response.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError("Applicant - Error : " + result);
            StateHasChanged();
            return;
        }
        else
        {
            VisiblePopup = false;
            _snackbar.AddSuccess($"Success insert data Study Period with ID {response.Result!.Id}");
            await SetGridStudyPeriod();
            StateHasChanged();
        }

        _isLoadingDialog = false;
        StateHasChanged(); // refresh tampilan setelah selesai

        #endregion insert study preparation
    }

    protected void ValueChangeUploaderTranscript(UploadChangeEventArgs args)
    {
        FileUploadTranscript.FileName = args.Files.First().File.Name;
        FileUploadTranscript.FileSize = args.Files.First().File.Size;
        FileUploadTranscript.FileContentType = args.Files.First().File.ContentType;
        FileUploadTranscript.File = args.Files.First().File;
    }

    protected void OnRemoveUploaderTranscript(RemovingEventArgs args)
    {
        FileUploadTranscript = new();
        args.PostRawFile = false;
    }

    private async Task<MemoryStream> UploadTranscript(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        await FileUploadTranscript.File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, FileUploadTranscript.File.Size, FileUploadTranscript.File.Name, FileUploadTranscript.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadTranscript.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadTranscript.File, memoryStream, FileUploadTranscript.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.Transcript,
            File = file,
            EmployeeNumber = employeeNumber,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentTranscriptId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }

    public async void OnCommandClicked(CommandClickEventArgs<GetStudyPeriodByAdmissionIdQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "TranscriptFile")
            {
                await OnClickDownload(args.RowData.TranscriptDocumentId, DocumentType.Transcript);
            }
        }
    }

    private async Task OnClickDownload(Guid documentId, DocumentType documentType)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = documentType,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError(result);

            return;
        }

        if (response.Result is null)
        {
            _snackbar.AddError($"Failed to retrieve the document {CommonDisplayTextFor.File}.");

            return;
        }

        await _jsRuntime!.InvokeVoidAsync(
            JavaScriptIdentifierFor.DownloadFile,
            response.Result.FileName,
            response.Result.ContentType,
            response.Result.Content);
    }
}
