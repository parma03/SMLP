/*
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Smlp.Shared.Applicants.Constants;

namespace Smlp.Bsui.Features.Applicants.Registration.Components;

public partial class RegistrationDetailExist
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(true));
        _navigationManager.NavigateTo(RouteFor.ApplicantSelectionCondition);
    }
}
