/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Universities.AdmissionSuperiorLetter.Commands.Create;
using Psmp.Shared.Universities.AdmissionSuperiorLetter.Queries;
using Psmp.Shared.Universities.UniversityRecommendation.Queries;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.Applicants.University;

public partial class UniversityAdmission
{
    #region Parameter
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected Properties
    public ErrorResponse? Error { get; set; }
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _isCanView = false;
    protected string _displayName = default!;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected Guid _lastApplicant = Guid.Empty;
    #endregion

    protected List<GetAdmissionSuperiorLetterByApplicantIdQueryResponse> _dataGrid = new();

    #region Modal Properties
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private bool VisiblePopup { get; set; }
    private bool _isButtonVisible = true; // Awalnya tombol terlihat
    private string TitlePopup { get; set; } = "Add";
    public List<GetUniversityRecommendationQueryResponse> _dropdownUnivesity = new();
    public List<DropdownItemResponse> _dropdownStudyTime = new();
    protected CreateAdmissionSuperiorLetterRequest _createAdmissionSuperiorLetterRequest = new();
    public Guid DocumentIdLoA { get; set; } = new();
    public FileUploadResponse FileUploadLoA { get; set; } = new();
    public Guid DocumentIdSuperiorApproval { get; set; } = new();
    public FileUploadResponse FileUploadSuperiorApproval { get; set; } = new();
    protected long _maximumFileSizeInBytes = long.MaxValue;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _displayName = _user.GetDisplayName() ?? string.Empty;

        _isLoading = true;
        _isCanView = false;

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

        // check if exist data scholarship
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error != null)
        {
            Error = applicant.Error;
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            if (lastApplicant is not null && lastApplicant!.Id != Guid.Empty)
            {
                _isCanView = true;
                _lastApplicant = lastApplicant!.Id;
                await SetDataOnLoadAsync();
                await OnLoadUniversity();
                await OnLoadStudyTime();
            }
        }

        _isLoading = false;
    }

    #region Data Grid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _universityAdmissionSuperiorLetterService.GetUniversityAdmissionSuperiorLetterByApplicantIdAsync(_lastApplicant);
        if (data.Error is null)
        {
            _dataGrid = data.Result!.Items.ToList();
            _isButtonVisible = _dataGrid.Count() > 0 ? false : true;
        }
        else
        {
            Error = data.Error;
        }
    }
    #endregion

    #region Dropdown
    private async Task OnLoadUniversity()
    {
        var datas = await _universityRecommendationService.GetUniversityRecommendationAsync();
        if (datas.Error is null)
        {
            _dropdownUnivesity = datas.Result!.Items.ToList();
            _dropdownUnivesity = _dropdownUnivesity.OrderBy(x => x.University).ToList();
        }

        await Task.CompletedTask;
    }
    private async Task OnLoadStudyTime()
    {
        _dropdownStudyTime = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "Full Time"},
            new() {Id = 2, Description = "Part Time"}
        };
        await Task.CompletedTask;
    }
    #endregion

    #region Action
    private void OnClickDetail(Guid id)
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Applicants.University.Constants.RouteFor.ApplicantUnivesityDetail("UniversityAdmission", id), true);
    }
    #endregion

    #region Modal
    private void OnClickAddAsync()
    {
        TitlePopup = "Add";
        ActionType = "add";
        IsEnabled = true;
        VisiblePopup = true;
        _createAdmissionSuperiorLetterRequest = new CreateAdmissionSuperiorLetterRequest { StudyStart = DateOnly.FromDateTime(DateTime.Now), StudyEnd = DateOnly.FromDateTime(DateTime.Now) };
        // Setelah berhasil Add, sembunyikan tombol
        _isButtonVisible = false;
    }

    private async Task<string> UploadDocumentLoA()
    {
        var memoryStream = new MemoryStream();
        await FileUploadLoA.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploadLoA.File.Size, FileUploadLoA.File.Name, FileUploadLoA.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadLoA.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadLoA.File, memoryStream, FileUploadLoA.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.AdmissionLetterOfAcceptance,
            File = file,
            EmployeeNumber = _user.GetEmployeeId()
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return string.Join(", ", resultUpload.Error.Details.ToList());
        }

        DocumentIdLoA = resultUpload.Result!.DocumentId;
        return string.Empty;
    }

    private async Task<string> UploadDocumentSuperiorApproval()
    {
        var memoryStream = new MemoryStream();
        await FileUploadSuperiorApproval.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploadSuperiorApproval.File.Size, FileUploadSuperiorApproval.File.Name, FileUploadSuperiorApproval.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadSuperiorApproval.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadSuperiorApproval.File, memoryStream, FileUploadSuperiorApproval.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.AdmissionSuperiorApprovalLetter,
            File = file,
            EmployeeNumber = _user.GetEmployeeId(),
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return string.Join(", ", resultUpload.Error.Details.ToList());
        }

        DocumentIdSuperiorApproval = resultUpload.Result!.DocumentId;
        return string.Empty;
    }

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        ActionType = "";
        VisiblePopup = false;
        _createAdmissionSuperiorLetterRequest = new();
    }
    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        if (FileUploadLoA.FileName is null)
        {
            _snackbar.AddWarning("Please browse Letter of Acceptance !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (FileUploadSuperiorApproval.FileName is null)
        {
            _snackbar.AddWarning("Please browse Superior Approval Letter !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (ActionType == "add")
        {
            #region Create
            var resUpload = await UploadDocumentLoA();
            if (!string.IsNullOrEmpty(resUpload))
            {
                _snackbar.AddError("Error : " + resUpload);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            resUpload = await UploadDocumentSuperiorApproval();
            if (!string.IsNullOrEmpty(resUpload))
            {
                _snackbar.AddError("Error : " + resUpload);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            var createData = new CreateAdmissionSuperiorLetterRequest
            {
                UniversityRecommendationId = _createAdmissionSuperiorLetterRequest.UniversityRecommendationId,
                ApplicantId = _lastApplicant,
                IsFulltime = _createAdmissionSuperiorLetterRequest.IsFulltime,
                StudyStart = _createAdmissionSuperiorLetterRequest.StudyStart,
                StudyEnd = _createAdmissionSuperiorLetterRequest.StudyEnd,
                Study = _createAdmissionSuperiorLetterRequest.Study,
                AcceptanceLetterDocumentId = DocumentIdLoA,
                ApprovalLetterDocumentId = DocumentIdSuperiorApproval
            };
            var createDataResponse = await _universityAdmissionSuperiorLetterService.CreateUniversityAdmissionSuperiorLetterAsync(createData);
            if (createDataResponse.Error is not null)
            {
                var result = string.Join(", ", createDataResponse.Error.Details.ToList());
                _snackbar.AddError("Applicant - Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            _snackbar.AddSuccess("Success add data");
            #endregion
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
        DialogClose();
        _isLoadingDialog = false;
    }

    private async Task ValueChangeUploaderLoA(UploadChangeEventArgs args)
    {
        FileUploadLoA.FileName = args.Files.First().File.Name;
        FileUploadLoA.FileSize = args.Files.First().File.Size;
        FileUploadLoA.FileContentType = args.Files.First().File.ContentType;
        FileUploadLoA.File = args.Files.First().File;

        await Task.CompletedTask;
    }

    protected void OnRemoveUploaderLoA(RemovingEventArgs args)
    {
        FileUploadLoA = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderSuperiorApproval(UploadChangeEventArgs args)
    {
        FileUploadSuperiorApproval.FileName = args.Files.First().File.Name;
        FileUploadSuperiorApproval.FileSize = args.Files.First().File.Size;
        FileUploadSuperiorApproval.FileContentType = args.Files.First().File.ContentType;
        FileUploadSuperiorApproval.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderSuperiorApproval(RemovingEventArgs args)
    {
        FileUploadSuperiorApproval = new();
        args.PostRawFile = false;
    }
    #endregion
}
