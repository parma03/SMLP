/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Features.Applicants.Selection.Models;
using Psmp.Shared.Applicants.Commands.Create;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Selections.SelectionTOEFLs.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Features.Applicants.Selection.Components;

public partial class DialogToeflSchedule
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public Guid ScholarshipPostingId { get; set; }

    [Parameter]
    public Guid ApplicantId { get; set; }

    private IList<GetSelectionTOEFLScheduleListResponse> ToeflSchedules { get; set; } = default!;
    private IList<GetSelectionTOEFLScheduleListResponse> ToeflScheduleSessions { get; set; } = default!;
    private IList<ScheduleTestModel> ScheduleTestModels { get; set; } = default!;
    private IList<SessionTestModel> SessionTestModels { get; set; } = default!;
    private GetSelectionTOEFLScheduleListResponse ToeflSchedule { get; set; } = new();
    private GetSelectionTOEFLScheduleListResponse ToeflScheduleSession { get; set; } = new();
    private CreateApplicantToeflScheduleRequest CreateRequest { get; set; } = default!;

    private Guid SelectionTOEFLScheduleId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var schedule = await _selectionTOEFLService.GetSelectionToeflSchedulelListAsync(ScholarshipPostingId);
        if (schedule.Error is null)
        {
            ToeflSchedules = new List<GetSelectionTOEFLScheduleListResponse>();
            ToeflSchedules = schedule.Result!.Items.Where(x => x.Status == Base.Selections.Enums.ScheduleStatus.NotStarted).ToList();
            ScheduleTestModels = new List<ScheduleTestModel>();
            var itemExisting = string.Empty;
            foreach (var item in ToeflSchedules)
            {
                var itemDate = DateOnly.FromDateTime(item.TestScheduleStart!.Value.DateTime).ToString("dd.MM.yyyy");
                if (itemDate != itemExisting)
                {
                    ScheduleTestModels.Add(new ScheduleTestModel
                    {
                        ScheduleId = item.Id,
                        ScheduleDate = itemDate
                    });
                }

                itemExisting = DateOnly.FromDateTime(item.TestScheduleStart!.Value.DateTime).ToString("dd.MM.yyyy");
            }
        }
    }

    protected void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(false));
    }

    protected async void OnClickYesAsync(MouseEventArgs args)
    {
        if (SelectionTOEFLScheduleId == Guid.Empty)
        {
            _snackbar.AddError(NotificationMessageFor.CannotEmpty(nameof(SelectionTOEFLScheduleId).SplitWords()));
            return;
        }

        var request = new CreateApplicantToeflScheduleRequest
        {
            ApplicantId = ApplicantId,
            SelectionTOEFLScheduleId = SelectionTOEFLScheduleId
        };

        var response = await _scheduleService.CreateApplicantToeflScheduleAsync(request);
        if (response.Error != null)
        {
            _snackbar.AddError(response.Error.Detail);
        }
        else
        {
            _snackbar.AddSuccess(SuccessMessageFor.Action("TOEFL Schedule", "Created"));
            MudDialog.Close(DialogResult.Ok(true));
        }
    }

    public void OnChangeToeflScheduleAsync(ChangeEventArgs<Guid, ScheduleTestModel> args)
    {
        SessionTestModels = new List<SessionTestModel>();
        foreach (var model in ToeflSchedules.Where(x => DateOnly.FromDateTime(x.TestScheduleStart!.Value.DateTime).ToString("dd.MM.yyyy").Equals(args.ItemData.ScheduleDate)).ToList())
        {
            SessionTestModels.Add(new SessionTestModel
            {
                ScheduleId = model.Id,
                Session = $"{model.Session} : {model.Session1Start.ToShortTimeString()} - {model.Session1End.ToShortTimeString()}"
            });
        }

        StateHasChanged();
    }

    public void OnChangeToeflScheduleSessionAsync(ChangeEventArgs<Guid, SessionTestModel> args)
    {
        if (args != null)
        {
            SelectionTOEFLScheduleId = args.ItemData.ScheduleId;
            StateHasChanged();
        }
    }
}
