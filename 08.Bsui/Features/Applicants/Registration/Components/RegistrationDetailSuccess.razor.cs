/*
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Smlp.Shared.Applicants.Constants;
using MudBlazor;

namespace Smlp.Bsui.Features.Applicants.Registration.Components;

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
