/*
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Smlp.Bsui.Features.Applicants.Registration.Components;

public partial class RegistrationDetailFailed
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}
