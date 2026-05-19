using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using Psmp.Base.Project.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Components;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Features.Projects.Constants;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Documents.Queries.GetDocuments;
using Psmp.Shared.Projects.ProjectCharterRealisations.Commands;
using Psmp.Shared.Projects.ProjectCharterRealisations.Queries;
using Psmp.Shared.Projects.ProjectComments.Commands;
using Psmp.Shared.Projects.ProjectComments.Queries;
using Syncfusion.Blazor.Grids;

namespace Psmp.Bsui.Features.Projects.ProjectCharterReal;

public partial class Edit
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    [Parameter]
    public string Id { get; set; } = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _dataExsist = false;
    protected string _displayName = default!;
    protected ProjectCharterRealizationRequest _request = new();
    protected long _maximumFileSizeInBytes;
    protected string _employeeId = string.Empty;
    protected string _userEmail = string.Empty;
    protected Guid? _documentId;
    protected Guid? _projectId;
    protected string _mentorId = string.Empty;
    protected string _mentorName = string.Empty;
    protected string _mentorEmail = string.Empty;
    protected string _mentorPosition = string.Empty;
    protected string _status = string.Empty;
    private List<GetProjectCommentResponse> _comments = new();
    private List<DropdownItemResponse> _dropdownItemMeasurement = new();
    private string _message = default!;
    private bool _isShowComment = false;
    protected List<GetDocumentsDocument> _getListDocuments = new();
    private bool _isShowApproveButton = true;
    protected string? _university = string.Empty;
    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();

        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _userEmail = _user.GetUsername() ?? string.Empty;
        _dropdownItemMeasurement = EnumExtentions.ToDropdownItem<ProjectCharterRealizationMeasurement>();
        #region bypass user applicant
        await LoadDataProjectCharterRealization(new Guid(Id));

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;
                _userEmail = asResponse.Result.EmailAlias ?? string.Empty;
                if (_request.MentorEmail == asResponse.Result.EmailAlias || _request.MentorEmail2 == asResponse.Result.EmailAlias)
                {
                    _isShowApproveButton = false;
                }
            }
            else
            {
                if (_request.MentorEmail == _user.GetUsername() || _request.MentorEmail2 == _user.GetUsername())
                {
                    _isShowApproveButton = false;
                }
            }
        }
        else
        {
            if (_request.MentorEmail == _user.GetUsername() || _request.MentorEmail2 == _user.GetUsername())
            {
                _isShowApproveButton = false;
            }
        }

        if (string.IsNullOrEmpty(_employeeId))
        {
            _snackbar.AddError("Not Found Employee ID");
            _isLoading = false;
            return;
        }

        #endregion bypass user applicant

        await LoadComment((Guid)_request.Id);
    }
    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Detail Project Charter Realization")
        };
    }
    private async Task LoadDataProjectCharterRealization(Guid applicantId)
    {
        var request = new GetProjectCharterRealizationRequest()
        {
            ApplicantId = applicantId,
        };
        var response = await _projectCharterRealisationService.GetProjectCharterRealisationsByApplicantIdAsync(request);
        if (response.Error is null)
        {
            _request.Id = response.Result.Id;
            _request.DocumentRealizationId = response.Result.DocumentRealizationId;
            _request.ProjectTitle = response.Result.ProjectTitle;
            _request.Abstract = response.Result.Abstract;
            _request.InnovationProject = response.Result.InnovationProject;
            _request.MentorEmail = response.Result.MentorEmail;
            _request.MentorId = response.Result.MentorId;
            _request.MentorName = response.Result.MentorName;
            _request.MentorEmail2 = response.Result.MentorEmail2;
            _request.MentorId2 = response.Result.MentorId2;
            _request.MentorName2 = response.Result.MentorName2;
            _request.Impact = response.Result.Impact;
            _request.Measurement = (int)response.Result.Measurement;
            _request.Realization = response.Result.Realization;
            _request.Target = response.Result.Target;
            _request.Status = (int)response.Result!.Status;
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

            await LoadDataApplicant();
        }
    }

    private async Task LoadDataApplicant()
    {
        var applicat = await _applicantService.GetApplicantByIdAsync(new Guid(Id));
        if (applicat.Error is null)
        {
            var data = applicat.Result!.Items.FirstOrDefault();
            if (data is not null)
            {
                _university = data!.UniversityApplicant;

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
            if (_comments.Where(x => x.User == _displayName).Any(x => x.Status == ProjectPlanStatus.Approved))
            {
                _isShowApproveButton = true;
            }
        }
    }
    private async Task SubmitComment()
    {
        var request = new ProjectCommentRequest()
        {
            ProjectId = (Guid)_request.Id,
            Comment = _message,
            User = _displayName,
            ProjectType = 4
        };

        var response = await _projectPlansService.CreateProjectPlanCommentAsync(request);
        if (response.Error is null)
        {
            _message = default!;
            await LoadComment((Guid)_request.Id);
        }
    }
    public async Task OnClickApprove()
    {
        var dialog = await DialogService.ShowAsync<DialogApproveValidation>("");

        var result = await dialog.Result;

        if (result.Canceled)
        {
            return;
        }

        var request = new ProjectCharterRealizationUpdateStatusRequest()
        {
            Id = (Guid)_request.Id,
            Status = 5,
            Comment = result!.Data as string ?? string.Empty,
            UserEmail = _userEmail,
            User = _displayName
        };
        var response = await _projectCharterRealisationService.ProjectCharterRealisationsUpdateStatus(request);
        if (response.Error is null)
        {
            _snackbar.AddSuccess($"Project Charter Realization is Approved");
            _navigationManager.NavigateTo(RouteFor.Project, true);
            await LoadDataProjectCharterRealization(new Guid(Id));
        }
    }
    public async Task OnClickReject()
    {
        var param = new DialogParameters
        {
            { "CondStatus", false }
        };
        var dialog = await DialogService.ShowAsync<DialogApproveValidation>("", param);

        var result = await dialog.Result;

        if (result.Canceled)
        {
            return;
        }

        var request = new ProjectCharterRealizationUpdateStatusRequest()
        {
            Id = (Guid)_request.Id,
            Status = 9,
            Comment = result!.Data as string ?? string.Empty,
            UserEmail = _userEmail,
            User = _displayName
        };
        var response = await _projectCharterRealisationService.ProjectCharterRealisationsUpdateStatus(request);
        if (response.Error is null)
        {
            _snackbar.AddSuccess($"Project Charter Realization is Rejected");
            _navigationManager.NavigateTo(RouteFor.Project, true);
            await LoadDataProjectCharterRealization(new Guid(Id));
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
    private void OnClickCancelAsync(MouseEventArgs e)
    {
        _navigationManager.NavigateTo(RouteFor.Project, true);
    }
}
