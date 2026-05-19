using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Smlp.Base.Selections.Enums;
using Smlp.Base.Studies.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.StudyPreparations.VisaGuarantee.Commands.Update;
using Smlp.Shared.StudyPreparations.VisaGuarantee.Queries;

namespace Smlp.Bsui.Features.StudyPreparations.VisaGuarantee;

public partial class Detail
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected Properties
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _isEnabled = false;
    protected bool _isEnabledReject = true;
    protected bool _isDisableButton = false;
    protected GetVisaGuaranteeByIdQueryResponse _dataItem = new();
    protected UpdateVisaGuaranteeStatusRequest _update = new();
    protected string? _supportingDocument;
    protected string? _bringFamily = "";
    #endregion

    #region Confirm
    private string TitlePopupConfirm { get; set; } = "";
    private bool VisiblePopupConfirm { get; set; } = false;
    private string TypeConfirm { get; set; } = "";
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        await OnLoadData();
        StateHasChanged();
    }

    #region OnLoadData
    private async Task OnLoadData()
    {
        _isLoading = true;

        var dataResponse = await _studyPreparationVisaGuaranteeService.GetStudyPreparationVisaGuaranteeByIdAsync(Id);
        if (dataResponse.Result == null || dataResponse.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(5), true);
            StateHasChanged();
            return;
        }

        _dataItem = dataResponse.Result;
        _bringFamily = _dataItem.IsBringFamily ? "Yes" : "No";
        switch (_dataItem.Status)
        {
            case VisaGuaranteeStatus.WaitingApproval:
                break;
            case VisaGuaranteeStatus.Accepted:
            case VisaGuaranteeStatus.Completed:
            case VisaGuaranteeStatus.ProcessSign:
            case VisaGuaranteeStatus.ProcessToPOffice:
            case VisaGuaranteeStatus.Draft:
            case VisaGuaranteeStatus.Rejected:
            default:
                _isEnabledReject = false;
                _isDisableButton = true;
                _update.Notes = _dataItem.Notes;
                break;
        }

        if (_dataItem.DocumentId != Guid.Empty)
        {
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _dataItem.DocumentId ?? Guid.Empty, DocumentType = DocumentType.VisaGuarantee });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _supportingDocument = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }

        _isLoading = false;
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion

    #region OnClick Event
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(5), true);
        await Task.CompletedTask;
    }

    private async Task OnClickContinueAsync()
    {
        if (string.IsNullOrEmpty(_update.Notes) || _update.Notes.Length == 0 || string.IsNullOrWhiteSpace(_update.Notes))
        {
            _snackbar.AddError("Please Add Notes");
            return;
        }

        VisiblePopupConfirm = true;
        TitlePopupConfirm = "<p style='padding:20px;'>Are you sure accepted this data?";
        TypeConfirm = "Accepted";
        await Task.CompletedTask;
    }

    private async Task OnClickRejectAsync()
    {
        if (string.IsNullOrEmpty(_update.Notes) || _update.Notes.Length == 0 || string.IsNullOrWhiteSpace(_update.Notes))
        {
            _snackbar.AddError("Please Add Notes");
            return;
        }

        VisiblePopupConfirm = true;
        TitlePopupConfirm = "<p style='padding:20px;'>Are you sure rejected this data?";
        TypeConfirm = "Rejected";
        await Task.CompletedTask;
    }
    #endregion

    #region Modal Confirm
    private void DialogConfirmOpen()
    {
        VisiblePopupConfirm = true;
    }

    private void DialogConfirmClose()
    {
        VisiblePopupConfirm = false;
    }

    private void ClickedNoConfirm()
    {
        VisiblePopupConfirm = false;
    }

    private async void ClickedYesConfirm()
    {
        if (_dataItem.Status != VisaGuaranteeStatus.WaitingApproval)
        {
            _snackbar.AddError("Error : Can not action this data!!");
            return;
        }

        _isLoading = true;

        var status = VisaGuaranteeStatus.WaitingApproval;
        if (TypeConfirm == "Accepted")
        {
            status = VisaGuaranteeStatus.Accepted;
        }
        else if (TypeConfirm == "Rejected")
        {
            status = VisaGuaranteeStatus.Rejected;
        }

        var updateData = new UpdateVisaGuaranteeStatusRequest
        {
            Id = _dataItem.Id,
            Status = status,
            Notes = _update.Notes
        };

        var updateDataResponse = await _studyPreparationVisaGuaranteeService.UpdateStudyPreparationVisaGuaranteeStatusAsync(updateData);
        if (updateDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        _snackbar.AddSuccess($"Success {TypeConfirm} data");

        if (TypeConfirm == "Accepted")
        {
            _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationDetailAccepted("VisaGuarantee", _dataItem.Id), true);
        }
        else if (TypeConfirm == "Rejected")
        {
            _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(5), true);
        }

        _isLoading = false;
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion
}
