/*
*/

using Microsoft.AspNetCore.Components;
using Smlp.Shared.Applicants.Constants;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Smlp.Base.Selections.Enums;

namespace Smlp.Bsui.Features.Applicants.Registration.Components;

public partial class RegistrationDetailExistInOther
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public string Code { get; set; } = default!;

    [Parameter]
    public SelectionStage? SelectionStage { get; set; } = default!;

    private void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(true));
        _navigationManager.NavigateTo(RouteFor.ApplicantSelectionCondition);
    }
}
