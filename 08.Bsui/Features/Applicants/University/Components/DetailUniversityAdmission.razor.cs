/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Universities.AdmissionSuperiorLetter.Queries;
using Psmp.Shared.Universities.Constants;
namespace Psmp.Bsui.Features.Applicants.University.Components;

public partial class DetailUniversityAdmission
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected Properties
    protected GetAdmissionSuperiorLetterByIdQueryResponse _data = new();
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isCanView = false;
    protected string _displayName = default!;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string? _documentPathLoA;
    protected string? _documentPathSuperiorApproval;
    #endregion
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;

        _breadcrumbItems.Clear();
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.University));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UniversityAdmission));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Detail));

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
            _error = applicant.Error;
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            if (lastApplicant!.Id != Guid.Empty)
            {
                _isCanView = true;
                await OnLoadData();
                await OnLoadDocument();
            }
        }

        _isLoading = false;
    }

    #region OnLoad Data
    public async Task OnLoadData()
    {
        var data = await _universityAdmissionSuperiorLetterService.GetUniversityAdmissionSuperiorLetterByIdAsync(Id);
        if (data.Result != null)
        {
            _data = data.Result;
        }

        await Task.CompletedTask;
    }

    public async Task OnLoadDocument()
    {
        if (_data is not null && _data.Id != Guid.Empty)
        {
            if (_data.AcceptanceLetterDocumentId != Guid.Empty)
            {
                // load document LoA
                var docLoA = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.AcceptanceLetterDocumentId ?? Guid.Empty, DocumentType = DocumentType.AdmissionLetterOfAcceptance });
                if (docLoA.Error is null)
                {
                    var dataDoc = docLoA.Result;
                    _documentPathLoA = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
                }
            }

            if (_data.ApprovalLetterDocumentId != Guid.Empty)
            {
                var docSuperiorApproval = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.ApprovalLetterDocumentId ?? Guid.Empty, DocumentType = DocumentType.AdmissionSuperiorApprovalLetter });
                if (docSuperiorApproval.Error is null)
                {
                    var dataDoc = docSuperiorApproval.Result;
                    _documentPathSuperiorApproval = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
                }
            }
        }
    }
    #endregion

    #region Action
    private void OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Applicants.University.Constants.RouteFor.ApplicantUnivesityTab(0), true);
    }
    #endregion
}
