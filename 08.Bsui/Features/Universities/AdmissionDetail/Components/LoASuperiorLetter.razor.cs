using Microsoft.AspNetCore.Components;
using Smlp.Base.Selections.Enums;
using Smlp.Base.Universities.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Universities.Admission.Commands.Update;
using Smlp.Shared.Universities.AdmissionSuperiorLetter.Queries;

namespace Smlp.Bsui.Features.Universities.AdmissionDetail.Components;

public partial class LoASuperiorLetter
{
    #region Parameter Properties
    [Parameter]
    public Guid Id { get; set; }
    #endregion

    #region Protected Properties
    protected GetAdmissionSuperiorLetterByAdmissionIdQueryResponse _data = new();
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _disableButton = false;
    protected string? _documentPathLoA;
    protected string? _documentPathSuperiorApproval;
    protected int _valIsFulltime = 1;

    protected bool _isVisibleConfirmation = false;
    protected string? _typeConfirmation = "";
    protected string? _titleConfirmation = "";
    protected string? _contentConfirmation = "";
    #endregion

    #region Private Properties
    private bool IsEnabled { get; set; } = false;
    #endregion

    #region Public Properties
    public List<DropdownItemResponse> _dropdownStudyTime = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await OnLoadData();
        await OnLoadDocument();
        await OnLoadStudyTime();
        await Task.CompletedTask;
        _isLoading = false;
    }

    #region OnLoad Data
    public async Task OnLoadData()
    {
        var data = await _universityAdmissionSuperiorLetterService.GetUniversityAdmissionSuperiorLetterByAdmissionIdAsync(Id);
        if (data.Result != null)
        {
            _data = data.Result;
            _valIsFulltime = _data.IsFulltime == true ? 1 : 2;

            _disableButton = _data.Status switch
            {
                AdmissionStatus.Submitted or AdmissionStatus.WaitingApproval => false,
                _ => true,
            };
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
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.Universities.Constants.RouteFor.UniversityAdmissionTab(1), true);
        await Task.CompletedTask;
    }
    #region Dropdown
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

    #region OnClick Event
    private async Task OnClickContinueAsync()
    {
        _isVisibleConfirmation = true;
        _typeConfirmation = "Approved";
        _titleConfirmation = "Approved";
        _contentConfirmation = "<p style='padding:20px;'>Are you sure approved this data?</p>";
        await Task.CompletedTask;
    }

    private async Task OnClickRejectAsync()
    {
        _isVisibleConfirmation = true;
        _typeConfirmation = "Rejected";
        _titleConfirmation = "Reject";
        _contentConfirmation = "<p style='padding:20px;'>Are you sure reject this data?</p>";
        await Task.CompletedTask;
    }
    #endregion

    #region Confirmation
    private void DialogConfirmationOpen()
    {
        _isVisibleConfirmation = true;
    }

    private void DialogConfirmationClose()
    {
        _isVisibleConfirmation = false;
    }

    private void ClickedNoConfirmation()
    {
        _isVisibleConfirmation = false;
    }

    private async Task ClickedYesConfirmation()
    {
        _isLoadingDialog = true;
        if (_typeConfirmation is not null)
        {
            var status = AdmissionStatus.WaitingApproval;
            switch (_typeConfirmation)
            {
                case "Approved":
                    status = AdmissionStatus.Approved;
                    break;

                case "Rejected":
                    status = AdmissionStatus.Approved;
                    break;
            }

            var updateData = new UpdateAdmissionStatusRequest
            {
                Id = _data.AdmissionId,
                Status = status
            };
            var updateDataResponse = await _universityAdmissionService.UpdateUniversityAdmissionStatusAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Applicant - Error : " + result);
                return;
            }
        }

        _isVisibleConfirmation = false;
        _typeConfirmation = "";
        _titleConfirmation = "";
        _contentConfirmation = "";
        _isLoadingDialog = false;
        _snackbar.AddSuccess("Success update data!");
        await OnLoadData();
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion
}
