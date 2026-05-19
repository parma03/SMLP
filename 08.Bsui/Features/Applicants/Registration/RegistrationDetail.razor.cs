/*
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;

namespace Smlp.Bsui.Features.Applicants.Registration;

public partial class RegistrationDetail
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public GetScholarshipPostingResponse Content { get; set; } = default!;

    private void OnClickApply(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void OnClickCancel(MouseEventArgs args)
    {
        MudDialog.Cancel();
    }
}
