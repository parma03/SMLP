/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components;
using MudBlazor;
using Psmp.Bsui.Features.Applicants.Selection.Models;

namespace Psmp.Bsui.Features.Applicants.Selection.Components;

public partial class SelectionStep
{
    [Parameter]
    public Guid ApplicantId { get; set; }

    [Parameter]
    public Guid ScholarshipId { get; set; }

    [Parameter]
    public IList<SelectionTimelineModel> StepperSource { get; set; } = default!;

    protected async Task OnClickStep(Base.Selections.Enums.SelectionStage stage, Guid applicantId, Guid scholarshipId, string selectionStageText)
    {
        var options = new DialogOptions { CloseOnEscapeKey = false, DisableBackdropClick = true };
        var parameters = new DialogParameters<DialogSelectionStep>
        {
            { x => x.ScholarshipId, scholarshipId },
            { x => x.ApplicantId, applicantId },
            { x => x.SelectionStage, stage },
            { x => x.SelectionStageText, selectionStageText }
        };

        //show dialog apply
        var dialog = await _dialogService.ShowAsync<DialogSelectionStep>("Application Selection", parameters, options);
        var result = await dialog.Result;
    }
}
