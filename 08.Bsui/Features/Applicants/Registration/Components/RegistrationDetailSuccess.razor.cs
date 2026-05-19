/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Psmp.Shared.Applicants.Constants;
using MudBlazor;

namespace Psmp.Bsui.Features.Applicants.Registration.Components;

public partial class RegistrationDetailSuccess
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(true));
        _navigationManager.NavigateTo(RouteFor.ApplicantSelectionCondition);
    }
}
