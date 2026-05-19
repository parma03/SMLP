/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Applicants.Commands.Cancel;
using Psmp.Shared.Applicants.Constants;

namespace Psmp.Bsui.Features.Applicants.Selection.Components;

public partial class DialogCancelApplicant
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public Guid ApplicantId { get; set; }

    protected string Reason { get; set; } = default!;

    protected const int MinimumTextReason = 10;

    private void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(true));
    }

    protected async void OnClickYesAsync(MouseEventArgs args)
    {
        if (string.IsNullOrEmpty(Reason))
        {
            _snackbar.AddError(NotificationMessageFor.CannotEmpty(nameof(Reason)));
            return;
        }

        if (Reason.Length < MinimumTextReason)
        {
            _snackbar.AddError(NotificationMessageFor.MinimumTextRequired(nameof(Reason)));
            return;
        }

        var request = new CancelApplicantRequest
        {
            ApplicantId = ApplicantId,
            Reason = Reason
        };
        var response = await _applicantService.CancelApplicantAsync(request);
        if (response.Error != null)
        {
            _snackbar.AddError(response.Error.Detail);
        }
        else
        {
            _snackbar.AddSuccess(SuccessMessageFor.Action("Applicant", "Canceled"));
            MudDialog.Close(DialogResult.Ok(true));
            _navigationManager.NavigateTo(RouteFor.Applicant, true);
        }
    }
}
