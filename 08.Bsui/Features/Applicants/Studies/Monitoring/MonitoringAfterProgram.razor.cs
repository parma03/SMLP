/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Psmp.Base.Selections.Enums;
using Psmp.Base.Studies.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.StudyMonitorings.AfterProgram.Commands.Update;
using Psmp.Shared.StudyMonitorings.AfterProgram.Queries;
using Psmp.Shared.StudyMonitorings.StudyMonitoringComment.Commands.Create;
using Psmp.Shared.StudyMonitorings.StudyMonitoringComment.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.Applicants.Studies.Monitoring;

public partial class MonitoringAfterProgram
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    public ErrorResponse? Error { get; set; }
    protected Guid ApplicantId { get; set; }
    protected Guid AdmissionId { get; set; }
    protected Guid AfterProgramId { get; set; }
    protected DateOnly PresentationDate { get; set; }
    protected TimeOnly PresentationTime { get; set; }
    protected Guid PresentationDocumentId { get; set; }
    protected string PresentationLink { get; set; } = default!;
    protected string PresentationStatus { get; set; } = default!;

    protected bool _isLoading = false;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string _displayName = default!;
    protected string _universityName = default!;
    private bool VisiblePopupFile { get; set; }
    protected FileUploadResponse FileUploadFile { get; set; } = new();
    protected Guid DocumentFileId { get; set; }
    protected long _maximumFileSizeInBytes = long.MaxValue;
    private SfGrid<GetAfterProgramQueryResponse> RefGrid { get; set; } = new();
    private IList<GetAfterProgramQueryResponse> _grid = new List<GetAfterProgramQueryResponse>();

    private IList<GetStudyMonitoringCommentsResponse> _comments = new List<GetStudyMonitoringCommentsResponse>();
    private string _message = default!;
    private bool _isShowComment = false;

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

                    var presentations = await _studyMonitoringAfterProgramService.GetAfterProgramAsync();
                    if (presentations.Error != null)
                    {
                        Error = presentations.Error;
                    }
                    else
                    {
                        var presentation = presentations.Result!.Items.Where(x => x.AdmissionId == AdmissionId).FirstOrDefault();

                        if (presentation != null)
                        {
                            AfterProgramId = presentation.Id;
                            PresentationDate = presentation.PresentationSchedule;
                            PresentationTime = presentation.PresentationScheduleTime;
                            PresentationLink = presentation.PresentationLink;
                            PresentationDocumentId = presentation.PresentationDocumentId ?? Guid.Empty;
                            PresentationStatus = presentation.Status.GetDescription();
                            _isShowComment = true;
                        }
                        else
                        {
                            AfterProgramId = Guid.Empty;
                            PresentationDate = DateOnly.MinValue;
                            PresentationTime = TimeOnly.MinValue;
                            PresentationLink = string.Empty;
                            PresentationDocumentId = Guid.Empty;
                            PresentationStatus = Psmp.Base.Studies.Enums.PresentationStatus.NotStarted.GetDescription();
                        }

                        await OnLoadGrid();
                    }
                }
            }
        }

        _isLoading = false;
    }

    private async Task OnLoadGrid()
    {
        var presentations = await _studyMonitoringAfterProgramService.GetAfterProgramAsync();
        if (presentations.Error != null)
        {
            Error = presentations.Error;
        }
        else
        {
            _grid = presentations.Result!.Items.Where(x => x.Id == AfterProgramId).ToList();
        }
    }

    #region Action
    public void RowBound(RowDataBoundEventArgs<GetAfterProgramQueryResponse> args)
    {
        if (args.Data.Status == Base.Studies.Enums.PresentationStatus.Done)
        {
            args.Row.AddClass(new string[] { "e-removeDeletecommand" });
        }
    }

    public async Task OnCommandClicked(CommandClickEventArgs<GetAfterProgramQueryResponse> args)
    {
        DialogCloseFile();
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            DialogOpenFile();
        }
        else if (args.CommandColumn.Type == CommandButtonType.None && args.CommandColumn.ButtonOption.Content.Equals("DownloadFile"))
        {
            if (args.RowData.PresentationDocumentId == Guid.Empty || args.RowData.PresentationDocumentId is null)
            {
                _snackbar.AddError("Error : Document not found!!");
                return;
            }

            await OnDownloadFile((Guid)args.RowData.PresentationDocumentId, DocumentType.AfterProgram);
            return;
        }
    }
    #endregion

    #region popup method

    private void DialogOpenFile()
    {
        VisiblePopupFile = true;
    }

    private void DialogCloseFile()
    {
        VisiblePopupFile = false;
    }

    private void ClickedNoFile()
    {
        DialogCloseFile();
    }

    private async Task ClickedYesFile()
    {
        StateHasChanged();

        if (string.IsNullOrEmpty(FileUploadFile.FileName))
        {
            _snackbar.AddWarning("Please browse Presentation Document !");
            StateHasChanged();
            return;
        }

        await UploadFile(_employeeId);

        if (DocumentFileId == Guid.Empty)
        {
            _snackbar.AddWarning("Please upload Presentation Document !");
            StateHasChanged();
            return;
        }

        var update = new UpdateAfterProgramRequest
        {
            Id = AfterProgramId,
            PresentationSchedule = PresentationDate,
            PresentationScheduleTime = PresentationTime,
            PresentationLink = PresentationLink,
            PresentationDocumentId = DocumentFileId,
            Status = Base.Studies.Enums.PresentationStatus.Done
        };

        PresentationDocumentId = update.PresentationDocumentId;

        var response = await _studyMonitoringAfterProgramService.UpdateAfterProgramAsync(update);
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
            VisiblePopupFile = false;
            _snackbar.AddSuccess($"Success upload data After Program!");
            StateHasChanged();
        }

        await OnLoadGrid();
        await RefGrid.Refresh();
        StateHasChanged();
    }

    protected void ValueChangeUploaderFile(UploadChangeEventArgs args)
    {
        FileUploadFile.FileName = args.Files.First().File.Name;
        FileUploadFile.FileSize = args.Files.First().File.Size;
        FileUploadFile.FileContentType = args.Files.First().File.ContentType;
        FileUploadFile.File = args.Files.First().File;
    }

    protected void OnRemoveUploaderFile(RemovingEventArgs args)
    {
        FileUploadFile = new();
        args.PostRawFile = false;
    }

    #endregion popup method

    #region Document
    private async Task<MemoryStream> UploadFile(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        await FileUploadFile.File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var formFile = new FormFile(memoryStream, 0, FileUploadFile.File.Size, FileUploadFile.File.Name, FileUploadFile.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadFile.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadFile.File, memoryStream, FileUploadFile.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.AfterProgram,
            File = file,
            EmployeeNumber = employeeNumber,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentFileId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }

    private async Task OnDownloadFile(Guid documentId, DocumentType docType)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = (DocumentType)docType,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            StateHasChanged();
            var result = string.Join(", ", response.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
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
    #endregion

    #region Comment
    private async Task SubmitComment()
    {
        var request = new CreateStudyMonitoringCommentRequest()
        {
            StudyMonitoringId = (Guid)AfterProgramId,
            Comment = _message,
            User = _displayName,
            CommentType = StudyMonitoringCommentType.AfterProgram
        };

        var response = await _studyMonitoringCommentService.CreateStudyMonitoringCommentAsync(request);
        if (response.Error is null)
        {
            _message = default!;
            await LoadComment(request.StudyMonitoringId);
            StateHasChanged();
        }
    }
    private async Task LoadComment(Guid id)
    {
        var request = new GetStudyMonitoringCommentsRequest()
        {
            StudyMonitoringId = id,
            CommentType = StudyMonitoringCommentType.AfterProgram
        };

        var response = await _studyMonitoringCommentService.GetStudyMonitoringsCommentAsync(request);
        if (response.Error is null)
        {
            _comments = response.Result!.Items.ToList();
        }
    }
    #endregion
}
