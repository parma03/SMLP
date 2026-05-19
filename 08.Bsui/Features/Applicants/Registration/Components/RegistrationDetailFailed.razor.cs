/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Psmp.Bsui.Features.Applicants.Registration.Components;

public partial class RegistrationDetailFailed
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}
