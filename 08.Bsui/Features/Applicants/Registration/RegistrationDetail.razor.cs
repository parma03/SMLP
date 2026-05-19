/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;

namespace Psmp.Bsui.Features.Applicants.Registration;

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
