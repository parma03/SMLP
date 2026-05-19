using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Projects.ProjectComments.Queries;
using System.Security.Claims;
using Smlp.Shared.Projects.ProjectCharterRealisations.Commands;
using Smlp.Shared.Projects.ProjectComments.Commands;
using Smlp.Shared.Projects.ProjectCharterRealisations.Queries;
using Smlp.Base.Selections.Enums;
using Syncfusion.Blazor.Inputs;
using Smlp.Shared.Documents.Commands.CreateDocument;
using System.ComponentModel;
using Smlp.Shared.Common.Extensions;
using Smlp.Base.Project.Enums;
using Smlp.Shared.Documents.Queries.GetDocuments;
using Syncfusion.Blazor.Grids;
using Microsoft.JSInterop;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Documents.Queries.GetDocumentFile;

namespace Smlp.Bsui.Features.Applicants.Projects.Components;
public partial class ProjectCharterRealisation
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _dataExsist = false;
    protected string _displayName = default!;
    protected ProjectCharterRealizationRequest _request = new();
    protected long _maximumFileSizeInBytes;
    protected string _employeeId = string.Empty;
    private Guid _applicantId = default!;
    protected Guid? _documentId;
    protected Guid? _projectCharterId;
    protected string _mentorId = string.Empty;
    protected string _mentorName = string.Empty;
    protected string _mentorEmail = string.Empty;
    protected string _mentorPosition = string.Empty;

    protected string _mentorId2 = string.Empty;
    protected string _mentorName2 = string.Empty;
    protected string _mentorEmail2 = string.Empty;
    protected string _mentorPosition2 = string.Empty;
    protected string? _university = string.Empty;
    private List<DropdownItemResponse> _dropdownItemMeasurement = new();
    public FileUploadResponse FileUploads { get; set; } = new();
    private List<GetProjectCommentResponse> _comments = new();
    private string _message = default!;
    private bool _isShowComment = false;
    private bool _isShowUpdate = true;
    private bool _isDisabledUpdate = false;
    protected List<GetDocumentsDocument> _getListDocuments = new();
    private string _status = default!;
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        _dropdownItemMeasurement = EnumExtentions.ToDropdownItem<ProjectCharterRealizationMeasurement>();
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
                _mentorId2 = data!.MentorId2;
                _mentorName2 = data!.MentorName2;
                _mentorPosition2 = data!.MentorPosition2;
                _mentorEmail2 = data!.MentorEmail2;
                _university = data!.UniversityApplicant;
                //await LoadDataMentor(_mentorPosition);
                await LoadMentorProject(_applicantId);
                await LoadDataProjectCharterRealization(_applicantId);
            }
        }
    }
    //public async Task LoadDataMentor(string? mentorPosition)
    //{
    //    if (string.IsNullOrEmpty(mentorPosition))
    //    {
    //        _snackbar.AddWarning("Please input employee number");
    //        return;
    //    }

    //    _isLoadingData = true;

    //    var dataHRIS = await _hrisService.GetEmployeeByPositionAsync(mentorPosition);
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
    private async Task LoadDataProjectCharterRealization(Guid applicantId)
    {
        var request = new GetProjectCharterRealizationRequest()
        {
            ApplicantId = applicantId,
            Id = _projectCharterId,
        };
        var response = await _projectCharterRealisationService.GetProjectCharterRealisationsByApplicantIdAsync(request);
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
                _request.Id = response.Result.Id;
                _request.DocumentRealizationId = response.Result.DocumentRealizationId;
                _request.ProjectTitle = response.Result.ProjectTitle;
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

                _request.Impact = response.Result.Impact;
                _request.Measurement = (int)response.Result.Measurement;
                _request.Realization = response.Result.Realization;
                _request.Target = response.Result.Target;
                _request.ImpactUnit = response.Result.ImpactUnit;
                _request.RealizationUnit = response.Result.RealizationUnit;
                _request.TargetUnit = response.Result.TargetUnit;

                _isShowComment = true;
                _status = response.Result.Status.GetDescription();
                if (response.Result!.DocumentRealization is not null)
                {
                    _getListDocuments.Add(new GetDocumentsDocument
                    {
                        Id = response.Result!.DocumentRealization!.Id,
                        FileName = response.Result!.DocumentRealization!.FileName,
                        DocumentType = DocumentType.ProjectCharterPlanRealitation,
                    });
                }

                if (_status == "Approved")
                {
                    _isShowUpdate = false;
                    _isDisabledUpdate = true;
                }

                _projectCharterId = response.Result.Id;
                await LoadComment(response.Result.Id);
            }
        }
    }
    private async Task LoadComment(Guid projectId)
    {
        var request = new GetProjectCommentRequest()
        {
            ProjectId = projectId,
            ProjectType = 4,
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
            ProjectId = (Guid)_projectCharterId,
            Comment = _message,
            User = _displayName,
            ProjectType = 4
        };

        var response = await _projectPlansService.CreateProjectPlanCommentAsync(request);
        if (response.Error is null)
        {
            _message = default!;
            await LoadComment((Guid)_projectCharterId);
        }
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
        var projectCharterPlan = _request;
        projectCharterPlan.ApplicantId = _applicantId;
        projectCharterPlan.Status = 1;
        if (_documentId is not null)
        {
            if (projectCharterPlan.Id is null)
            {
                projectCharterPlan.DocumentRealizationId = (Guid)_documentId;
                projectCharterPlan.MentorPosition = _mentorPosition;
                var response = await _projectCharterRealisationService.CreateProjectCharterRealisationAsync(projectCharterPlan);

                if (response.Error is not null)
                {
                    _snackbar.AddError(response.Error.Detail);
                }
                else
                {
                    _snackbar.AddSuccess($"Success Create Project Charter Plan");
                    StateHasChanged();
                }
            }
            else
            {
                if (FileUploads.File is not null)
                {
                    projectCharterPlan.DocumentRealizationId = (Guid)_documentId;
                    projectCharterPlan.MentorPosition = _mentorPosition;
                }

                var response = await _projectCharterRealisationService.UpdateProjectCharterRealisationAsync(projectCharterPlan);

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
            DocumentType = DocumentType.ProjectCharterPlanRealitation,
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
                DocumentType = DocumentType.ProjectCharterPlanRealitation,
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
    public static List<DropdownItemResponse> ToDropdownItem<T>()
    {
        var enumType = typeof(T);

        if (enumType.BaseType != typeof(Enum))
        {
            throw new ArgumentException("T is not System.Enum");
        }

        var list = new List<DropdownItemResponse>();

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            var info = item.GetType().GetField(item.ToString() ?? string.Empty);
            var attributes = (DescriptionAttribute[])info!.GetCustomAttributes(typeof(DescriptionAttribute), false);

            list.Add(new DropdownItemResponse
            {
                Id = (int)item,
                Description = (attributes is not null ? ((attributes.Length > 0) ? attributes[0].Description : item.ToString()) : string.Empty) ?? ""
            });
        }

        return list;
    }

    private readonly Dictionary<string, object> _units = new()
    {
        { "maxlength", "5" }
    };
}
