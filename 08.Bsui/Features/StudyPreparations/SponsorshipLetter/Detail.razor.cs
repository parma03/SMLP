using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Psmp.Base.Selections.Enums;
using Psmp.Base.Studies.Enums;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.StudyPreparations.SponsorshipLetter.Commands.Update;
using Psmp.Shared.StudyPreparations.SponsorshipLetter.Queries;

namespace Psmp.Bsui.Features.StudyPreparations.SponsorshipLetter;

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
    protected GetSponsorshipLetterByIdQueryResponse _dataItem = new();
    protected UpdateSponsorshipLetterStatusRequest _update = new();
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

        var dataResponse = await _studyPreparationSponsorshipLetterService.GetStudyPreparationSponsorshipLetterByIdAsync(Id);
        if (dataResponse.Result == null || dataResponse.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
            StateHasChanged();
            return;
        }

        _dataItem = dataResponse.Result;
        _bringFamily = _dataItem.IsBringFamily ? "Yes" : "No";
        switch (_dataItem.Status)
        {
            case SponsorshipLetterStatus.WaitingApproval:
                break;
            case SponsorshipLetterStatus.Accepted:
            case SponsorshipLetterStatus.Completed:
            case SponsorshipLetterStatus.ProcessSign:
            case SponsorshipLetterStatus.ProcessToPOffice:
            case SponsorshipLetterStatus.Draft:
            case SponsorshipLetterStatus.Rejected:
            default:
                _isEnabledReject = false;
                _isDisableButton = true;
                _update.Notes = _dataItem.Notes;
                break;
        }

        if (_dataItem.DocumentId != Guid.Empty)
        {
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _dataItem.DocumentId ?? Guid.Empty, DocumentType = DocumentType.SponsorshipLetter });
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
        _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
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
        if (_dataItem.Status != SponsorshipLetterStatus.WaitingApproval)
        {
            _snackbar.AddError("Error : Can not action this data!!");
            return;
        }

        _isLoading = true;

        var status = SponsorshipLetterStatus.WaitingApproval;
        if (TypeConfirm == "Accepted")
        {
            status = SponsorshipLetterStatus.Accepted;
        }
        else if (TypeConfirm == "Rejected")
        {
            status = SponsorshipLetterStatus.Rejected;
        }

        var updateData = new UpdateSponsorshipLetterStatusRequest
        {
            Id = _dataItem.Id,
            Status = status,
            Notes = _update.Notes
        };

        var updateDataResponse = await _studyPreparationSponsorshipLetterService.UpdateStudyPreparationSponsorshipLetterStatusAsync(updateData);
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
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationDetailAccepted("SponsorshipLetter", _dataItem.Id), true);
        }
        else if (TypeConfirm == "Rejected")
        {
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
        }

        _isLoading = false;
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion
}
