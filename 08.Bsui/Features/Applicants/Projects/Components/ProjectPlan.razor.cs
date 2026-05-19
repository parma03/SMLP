using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Smlp.Shared.Common.Responses;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Projects.ProjectPlans.Commands;
using Smlp.Shared.Common.Constants;
using Smlp.Base.Selections.Enums;
using Syncfusion.Blazor.Inputs;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Projects.ProjectComments.Queries;
using Smlp.Shared.Projects.ProjectComments.Commands;
using Smlp.Shared.Documents.Queries.GetDocuments;
using Syncfusion.Blazor.Grids;
using Microsoft.JSInterop;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Bsui.Features.Applicants.Projects.Components;

public partial class ProjectPlan
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _dataExsist = false;
    protected string _displayName = default!;
    protected ProjectPlanRequest _request = new();
    protected long _maximumFileSizeInBytes;
    protected string _employeeId = string.Empty;
    private Guid _applicantId = default!;
    protected Guid? _documentId;
    protected Guid? _projectId;
    protected string _mentorId = string.Empty;
    protected string _mentorName = string.Empty;
    protected string _mentorEmail = string.Empty;
    protected string _mentorPosition = string.Empty;

    protected string _mentorId2 = string.Empty;
    protected string _mentorName2 = string.Empty;
    protected string _mentorEmail2 = string.Empty;
    protected string _mentorPosition2 = string.Empty;
    public FileUploadResponse FileUploads { get; set; } = new();
    private List<GetProjectCommentResponse> _comments = new();
    private string _message = default!;
    private string _status = default!;
    private bool _isShowComment = false;
    private bool _isShowUpdate = true;
    private bool _isDisabledUpdate = false;
    protected string? _university = string.Empty;
    protected List<GetDocumentsDocument> _getListDocuments = new();
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;

        #region bypass user applicant

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;

            }
        }

        if (string.IsNullOrEmpty(_employeeId))
        {
            _snackbar.AddError("Not Found Employee ID");
            _isLoading = false;
            return;
        }
        #endregion bypass user applicant

        var applicat = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicat.Error is null)
        {
            var data = applicat.Result!.Items.FirstOrDefault();
            if (data is not null)
            {
                _applicantId = data!.Id;
                _mentorId = data!.MentorId;
                _mentorName = data!.MentorName;
                _mentorPosition = data!.MentorPosition;
                _mentorEmail = data!.MentorEmail;
                _university = data!.UniversityApplicant;
                _request.InnovationProject = data!.InnovationProject;

                await LoadMentorProject(_applicantId);
                await LoadDataProjectPlan(_applicantId);
            }
        }
    }
    //public async Task LoadDataMentor(string? mentorIdForSearch)
    //{
    //    if (string.IsNullOrEmpty(mentorIdForSearch))
    //    {
    //        _snackbar.AddWarning("Please input employee number");
    //        return;
    //    }

    //    _isLoadingData = true;

    //    var dataHRIS = await _hrisService.GetEmployeeByPositionAsync(_mentorPosition);
    //    if (dataHRIS is not null)
    //    {
    //        _mentorId = dataHRIS.EmployeeId;
    //        _mentorName = dataHRIS.EmployeeName;
    //        _mentorEmail = dataHRIS.Email;
    //        _snackbar.AddInfo("Success retrieve data from HRIS");
    //    }
    //    else
    //    {
    //        _snackbar.AddError("Employee not found in HRIS!");
    //    }

    //    _isLoadingData = false;
    //}
    private async Task LoadMentorProject(Guid applicantId)
    {
        var response = await _selectionTalentReviewService.GetSelectionTalentReviewByApplicantIdAsync(applicantId);
        if (response.Error is null)
        {
            var data = response.Result;
            _mentorId2 = data!.MentorId2;
            _mentorName2 = data!.MentorName2;
            _mentorPosition2 = data!.MentorPosition2;
            _mentorEmail2 = data!.MentorEmail2;
            _request.MentorName2 = data.MentorName2;
            _request.MentorEmail2 = data!.MentorEmail2;
            _request.MentorId2 = data!.MentorId2;
            _request.MentorPosition2 = data!.MentorPosition2;
        }
    }

    private async Task LoadDataProjectPlan(Guid applicantId)
    {
        var response = await _projectPlansService.GetProjectPlanByApplicantIdAsync(applicantId);
        if (response.Error is null)
        {
            if (response.Result is null)
            {
                //_request.ProjectTitle = response.Result.Title;
                //_request.Abstract = response.Result.Abstract;
                //_request.InnovationProject = response.Result.InnovationProject;
                _request.MentorName = _mentorName;
                _request.MentorEmail = _mentorEmail;
                _request.MentorId = _mentorId;
                _request.MentorPosition = _mentorPosition;
                _status = "Not Submited";
            }
            else
            {
                _request.Id = response.Result!.Id;
                _request.ProjectTitle = response.Result.Title;
                _request.Abstract = response.Result.Abstract;
                _request.InnovationProject = response.Result.InnovationProject;
                _request.MentorName = response.Result!.MentorName;
                _request.MentorEmail = response.Result!.MentorEmail;
                _request.MentorId = response.Result!.MentorId;
                _request.MentorPosition = response.Result!.MentorPosition;

                _request.MentorName2 = response.Result!.MentorName2;
                _request.MentorEmail2 = response.Result!.MentorEmail2;
                _request.MentorId2 = response.Result!.MentorId2;
                _request.MentorPosition2 = response.Result!.MentorPosition2;

                _request.DocumentProposalId = response.Result!.DocumentProposalId;
                _isShowComment = true;
                _status = response.Result.Status.GetDescription();
                if (response.Result!.DocumentProposal is not null)
                {
                    _getListDocuments.Add(new GetDocumentsDocument
                    {
                        Id = response.Result!.DocumentProposal!.Id,
                        FileName = response.Result!.DocumentProposal!.FileName,
                        DocumentType = DocumentType.ProjectPlan,
                    });
                }

                if (_status == "Approved")
                {
                    _isShowUpdate = false;
                    _isDisabledUpdate = true;
                }

                _projectId = response.Result.Id;
                await LoadComment(response.Result.Id);
            }
        }
    }

    private async Task LoadComment(Guid projectId)
    {
        var request = new GetProjectCommentRequest()
        {
            ProjectId = projectId,
            ProjectType = 1,
        };

        var response = await _projectPlansService.GetProjectPlanCommentAsync(request);
        if (response.Error is null)
        {
            _comments = response.Result!.Items.ToList();
        }
    }
    private async Task SubmitComment()
    {
        var request = new ProjectCommentRequest()
        {
            ProjectId = (Guid)_projectId,
            Comment = _message,
            User = _displayName,
            ProjectType = 1
        };

        var response = await _projectPlansService.CreateProjectPlanCommentAsync(request);
        if (response.Error is null)
        {
            _message = default!;
            await LoadComment((Guid)_projectId);
        }
    }
    public async void OnCommandClickedDocument(CommandClickEventArgs<GetDocumentsDocument> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "Download")
            {
                await OnClickDownload(args.RowData.Id);
            }
            //else if (args.CommandColumn.Title == "Delete")
            //{
            //    _documentId = args.RowData.Id;
            //    VisiblePopupDelete = true;
            //    DeleteType = "DeleteDocument";
            //}

        }
    }
    private async Task OnClickDownload(Guid documentId)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = DocumentType.ProjectPlan,
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
    private async Task OnClickSave()
    {
        if (_request.InnovationProject == null)
        {
            _snackbar.AddWarning("Please input inovation project!");
            StateHasChanged();
            return;
        }

        else if (_request.ProjectTitle == null)
        {
            _snackbar.AddWarning("Please input project title");
            StateHasChanged();
            return;
        }

        else if (_request.Abstract == null)
        {
            _snackbar.AddWarning("Please input abstract");
            StateHasChanged();
            return;
        }

        else if (FileUploads.File == null)
        {
            _snackbar.AddWarning("Please input document");
            StateHasChanged();
            return;
        }

        await UploadReimbursement(_employeeId);
        var projectPlan = _request;
        projectPlan.ApplicantId = _applicantId;
        projectPlan.Status = 1;
        if (_documentId is not null)
        {
            if (projectPlan.Id is null)
            {
                projectPlan.DocumentProposalId = (Guid)_documentId;
                projectPlan.MentorPosition = _mentorPosition;
                var response = await _projectPlansService.CreateProjectPlanAsync(projectPlan);

                if (response.Error is not null)
                {
                    _snackbar.AddError(response.Error.Detail);
                }
                else
                {
                    _snackbar.AddSuccess($"Success Create Project Plan");
                    StateHasChanged();
                }
            }
            else
            {
                if (FileUploads.File is not null)
                {
                    projectPlan.MentorPosition = _mentorPosition;
                    projectPlan.DocumentProposalId = (Guid)_documentId;
                }

                var response = await _projectPlansService.UpdateProjectPlanAsync(projectPlan);

                if (response.Error is not null)
                {
                    _snackbar.AddError(response.Error.Detail);
                }
                else
                {
                    _snackbar.AddSuccess($"Success Update Project Palan");
                    StateHasChanged();
                }

            }
        }

        _navigationManager.NavigateTo(CommonRouteFor.ProjectApplicant, true);
    }
    protected void ValueChangeProjectProfosal(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void OnRemoveProjectProfosal(RemovingEventArgs args)
    {
        FileUploads = new();
        args.PostRawFile = false;
    }
    private async Task<MemoryStream> UploadReimbursement(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        if (FileUploads.File is not null)
        {
            await FileUploads.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset the stream position

            var formFile = new FormFile(memoryStream, 0, FileUploads.File.Size, FileUploads.File.Name, FileUploads.File.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = FileUploads.File.ContentType
            };
            var file = IBrowserFileExtensions.ToFormFile(FileUploads.File, memoryStream, FileUploads.File.Name);

            var requestFile = new CreateDocumentRequest
            {
                DocumentType = DocumentType.ProjectPlan,
                File = file,
                EmployeeNumber = employeeNumber,
            };

            var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
            if (resultUpload.Error is null)
            {
                _documentId = resultUpload.Result!.DocumentId;
                //_updateSelectionTOEFLRequest.DocumentId = resultUpload.Result!.DocumentId;
            }
            else
            {
                _snackbar.AddError(resultUpload.Error.Detail);
            }
        }

        return memoryStream;

    }
}
