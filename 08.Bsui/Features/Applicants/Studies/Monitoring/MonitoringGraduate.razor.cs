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
using Smlp.Shared.StudyMonitorings.Graduate.Commands.Create;
using Smlp.Shared.StudyMonitorings.Graduate.Queries;
using Smlp.Shared.StudyMonitorings.StudyPeriod.Queries;
using Syncfusion.Blazor.Inputs;

namespace Smlp.Bsui.Features.Applicants.Studies.Monitoring;

public partial class MonitoringGraduate
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    public ErrorResponse? Error { get; set; }
    protected Guid ApplicantId { get; set; }
    protected Guid AdmissionId { get; set; }
    protected Guid DocumentLastExamId { get; set; }
    protected Guid DocumentGraduateId { get; set; }
    protected Guid DocumentTranscriptId { get; set; }
    protected Guid DocumentCompletionId { get; set; }
    protected Guid DocumentFinalProjectId { get; set; }
    private bool VisiblePopup { get; set; }
    protected FileUploadResponse FileUploadLastExam { get; set; } = new();
    protected FileUploadResponse FileUploadGraduate { get; set; } = new();
    protected FileUploadResponse FileUploadTranscript { get; set; } = new();
    protected FileUploadResponse FileUploadCompletion { get; set; } = new();
    protected FileUploadResponse FileUploadFinalProject { get; set; } = new();
    public GetGraduateQueryResponse? GraduateData { get; set; } = new();

    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string _displayName = default!;
    protected string _universityName = default!;
    protected float _gpaMaximum = 0;
    protected float _gpaMinimum = 0;
    protected float _gpaAverage = 0;
    protected IList<GetStudyPeriodByAdmissionIdQueryResponse> _dataGrid = default!;
    public CreateGraduateRequest? _requestGraduate = new();
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

                    await SetCardStatistic();
                    await SetCardDocument();
                }
            }
        }

        _isLoading = false;
    }

    protected async Task SetCardStatistic()
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

    protected async Task SetCardDocument()
    {
        var documents = await _studyMonitoringGraduateService.GetGraduateAsync();
        if (documents.Error != null)
        {
            Error = documents.Error;
        }
        else
        {
            GraduateData = documents.Result!.Items.Where(x => x.AdmissionId == AdmissionId).FirstOrDefault() ?? new GetGraduateQueryResponse();
        }
    }

    private void OnClickAddAsync()
    {
        DialogOpen();
    }

    #region popup method

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        _requestGraduate = new CreateGraduateRequest();
        VisiblePopup = false;
    }

    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        StateHasChanged();
        #region upload document

        if (string.IsNullOrEmpty(FileUploadLastExam.FileName))
        {
            _snackbar.AddWarning("Please browse Last Exam !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (string.IsNullOrEmpty(FileUploadGraduate.FileName))
        {
            _snackbar.AddWarning("Please browse Graduate !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (string.IsNullOrEmpty(FileUploadTranscript.FileName))
        {
            _snackbar.AddWarning("Please browse Transcript !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (string.IsNullOrEmpty(FileUploadCompletion.FileName))
        {
            _snackbar.AddWarning("Please browse Certificate of Completion !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (string.IsNullOrEmpty(FileUploadFinalProject.FileName))
        {
            _snackbar.AddWarning("Please browse Final Project !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        await UploadLastExam(_employeeId);
        await UploadGraduate(_employeeId);
        await UploadTranscript(_employeeId);
        await UploadCompletion(_employeeId);
        await UploadFinalProject(_employeeId);
        await UploadFinalProject(_employeeId);

        if (DocumentLastExamId == Guid.Empty)
        {
            _snackbar.AddWarning("Please upload Last Exam !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (DocumentGraduateId == Guid.Empty)
        {
            _snackbar.AddWarning("Please upload Graduation !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (DocumentTranscriptId == Guid.Empty)
        {
            _snackbar.AddWarning("Please upload Transcript !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (DocumentCompletionId == Guid.Empty)
        {
            _snackbar.AddWarning("Please upload Certificate of Completion !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (DocumentFinalProjectId == Guid.Empty)
        {
            _snackbar.AddWarning("Please upload Final Project !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        #endregion upload document

        #region insert study preparation

        _requestGraduate!.AdmissionId = AdmissionId;
        _requestGraduate.LastExamDocumentId = DocumentLastExamId;
        _requestGraduate.GraduationDocumentId = DocumentGraduateId;
        _requestGraduate.TranscriptDocumentId = DocumentTranscriptId;
        _requestGraduate.CompletionDocumentId = DocumentCompletionId;
        _requestGraduate.CourseFinalDocumentId = DocumentFinalProjectId;
        _requestGraduate.GpaFinalScore = _gpaAverage;

        var response = await _studyMonitoringGraduateService.CreateGraduateAsync(_requestGraduate);
        if (response.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError("Applicant - Error : " + result);
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }
        else
        {
            VisiblePopup = false;
            _snackbar.AddSuccess($"Success insert data Graduate with ID {response.Result!.Id}");
            await SetCardDocument();
            StateHasChanged();
        }

        #endregion insert study preparation
        _isLoadingDialog = false;
        StateHasChanged();
    }

    #endregion popup method

    #region upload method

    protected void ValueChangeUploaderLastExam(UploadChangeEventArgs args)
    {
        FileUploadLastExam.FileName = args.Files.First().File.Name;
        FileUploadLastExam.FileSize = args.Files.First().File.Size;
        FileUploadLastExam.FileContentType = args.Files.First().File.ContentType;
        FileUploadLastExam.File = args.Files.First().File;
    }

    protected void OnRemoveUploaderLastExam(RemovingEventArgs args)
    {
        FileUploadLastExam = new();
        args.PostRawFile = false;
    }

    protected void ValueChangeUploaderGraduate(UploadChangeEventArgs args)
    {
        FileUploadGraduate.FileName = args.Files.First().File.Name;
        FileUploadGraduate.FileSize = args.Files.First().File.Size;
        FileUploadGraduate.FileContentType = args.Files.First().File.ContentType;
        FileUploadGraduate.File = args.Files.First().File;
    }

    protected void OnRemoveUploaderGraduate(RemovingEventArgs args)
    {
        FileUploadGraduate = new();
        args.PostRawFile = false;
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

    protected void ValueChangeUploaderCompletion(UploadChangeEventArgs args)
    {
        FileUploadCompletion.FileName = args.Files.First().File.Name;
        FileUploadCompletion.FileSize = args.Files.First().File.Size;
        FileUploadCompletion.FileContentType = args.Files.First().File.ContentType;
        FileUploadCompletion.File = args.Files.First().File;
    }

    protected void OnRemoveUploaderCompletion(RemovingEventArgs args)
    {
        FileUploadCompletion = new();
        args.PostRawFile = false;
    }

    protected void ValueChangeUploaderFinalProject(UploadChangeEventArgs args)
    {
        FileUploadFinalProject.FileName = args.Files.First().File.Name;
        FileUploadFinalProject.FileSize = args.Files.First().File.Size;
        FileUploadFinalProject.FileContentType = args.Files.First().File.ContentType;
        FileUploadFinalProject.File = args.Files.First().File;
    }

    protected void OnRemoveUploaderFinalProject(RemovingEventArgs args)
    {
        FileUploadFinalProject = new();
        args.PostRawFile = false;
    }

    private async Task<MemoryStream> UploadLastExam(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        await FileUploadLastExam.File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, FileUploadLastExam.File.Size, FileUploadLastExam.File.Name, FileUploadLastExam.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadLastExam.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadLastExam.File, memoryStream, FileUploadLastExam.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.LastExam,
            File = file,
            EmployeeNumber = employeeNumber,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentLastExamId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }

    private async Task<MemoryStream> UploadGraduate(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        await FileUploadGraduate.File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, FileUploadGraduate.File.Size, FileUploadGraduate.File.Name, FileUploadGraduate.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadGraduate.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadGraduate.File, memoryStream, FileUploadGraduate.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.Graduation,
            File = file,
            EmployeeNumber = employeeNumber,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentGraduateId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
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

    private async Task<MemoryStream> UploadCompletion(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        await FileUploadCompletion.File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, FileUploadCompletion.File.Size, FileUploadCompletion.File.Name, FileUploadCompletion.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadCompletion.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadCompletion.File, memoryStream, FileUploadCompletion.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.Completion,
            File = file,
            EmployeeNumber = employeeNumber,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentCompletionId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }

    private async Task<MemoryStream> UploadFinalProject(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        await FileUploadFinalProject.File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, FileUploadFinalProject.File.Size, FileUploadFinalProject.File.Name, FileUploadFinalProject.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadFinalProject.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadFinalProject.File, memoryStream, FileUploadFinalProject.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.FinalProject,
            File = file,
            EmployeeNumber = employeeNumber,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentFinalProjectId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }

    #endregion upload method

    #region download document

    protected async Task OnClickDownloadLastExam()
    {
        await Download(GraduateData!.LastExamDocumentId!.Value, DocumentType.LastExam);
    }

    protected async Task OnClickDownloadGraduate()
    {
        await Download(GraduateData!.GraduationDocumentId!.Value, DocumentType.Graduation);
    }

    protected async Task OnClickDownloadTranscript()
    {
        await Download(GraduateData!.TranscriptDocumentId!.Value, DocumentType.Transcript);
    }

    protected async Task OnClickDownloadCompletion()
    {
        await Download(GraduateData!.CompletionDocumentId!.Value, DocumentType.Completion);
    }

    protected async Task OnClickDownloadFinalProject()
    {
        await Download(GraduateData!.CourseFinalDocumentId!.Value, DocumentType.FinalProject);
    }

    private async Task Download(Guid documentId, DocumentType documentType)
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

    #endregion download document
}
