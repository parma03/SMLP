/*
*/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Applicants.Selection.Models;
using Smlp.Shared.Applicants.Commands.Create;
using Smlp.Shared.Selections.SelectionPsychotests.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Smlp.Bsui.Features.Applicants.Selection.Components;

public partial class DialogPsychotestSchedule
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public Guid ScholarshipPostingId { get; set; }

    [Parameter]
    public Guid ApplicantId { get; set; }

    private IList<GetSelectionPsychotestScheduleListResponse> PsychotestSchedules { get; set; } = default!;
    private IList<GetSelectionPsychotestScheduleListResponse> PsychotestScheduleSessions { get; set; } = default!;
    private IList<ScheduleTestModel> ScheduleTestModels { get; set; } = default!;
    private IList<SessionTestModel> SessionTestModels { get; set; } = default!;
    private GetSelectionPsychotestScheduleListResponse PsychotestSchedule { get; set; } = new();
    private GetSelectionPsychotestScheduleListResponse PsychotestScheduleSession { get; set; } = new();
    private CreateApplicantPsychotestScheduleRequest CreateRequest { get; set; } = default!;
    private Guid SelectionPsychotestScheduleId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var schedule = await _selectionPsychotestService.GetSelectionPsychotestSchedulelListAsync(ScholarshipPostingId);
        if (schedule.Error is null)
        {
            PsychotestSchedules = new List<GetSelectionPsychotestScheduleListResponse>();
            PsychotestSchedules = schedule.Result!.Items.Where(x => x.Status == Base.Selections.Enums.ScheduleStatus.NotStarted).ToList();
            ScheduleTestModels = new List<ScheduleTestModel>();
            var itemExisting = string.Empty;
            foreach (var item in PsychotestSchedules)
            {
                var itemDate = DateOnly.FromDateTime(item.ScheduleStartDate!.Value.DateTime).ToString("dd.MM.yyyy");
                if (itemDate != itemExisting)
                {
                    ScheduleTestModels.Add(new ScheduleTestModel
                    {
                        ScheduleId = item.Id,
                        ScheduleDate = itemDate
                    });
                }

                itemExisting = DateOnly.FromDateTime(item.ScheduleStartDate!.Value.DateTime).ToString("dd.MM.yyyy");
            }
        }
    }

    protected void OnClickClose(MouseEventArgs args)
    {
        MudDialog.Close(DialogResult.Ok(false));
    }

    protected async void OnClickYesAsync(MouseEventArgs args)
    {
        if (SelectionPsychotestScheduleId == Guid.Empty)
        {
            _snackbar.AddError(NotificationMessageFor.CannotEmpty(nameof(ScholarshipPostingId)));
            return;
        }

        var request = new CreateApplicantPsychotestScheduleRequest
        {
            ApplicantId = ApplicantId,
            SelectionPsychotestScheduleId = SelectionPsychotestScheduleId
        };

        var response = await _scheduleService.CreateApplicantPsychotestScheduleAsync(request);
        if (response.Error != null)
        {
            _snackbar.AddError(response.Error.Detail);
        }
        else
        {
            _snackbar.AddSuccess(SuccessMessageFor.Action("Psychotest Schedule", "Created"));
            MudDialog.Close(DialogResult.Ok(true));
        }
    }

    public void OnChangePsychotestScheduleAsync(ChangeEventArgs<Guid, ScheduleTestModel> args)
    {
        SessionTestModels = new List<SessionTestModel>();
        foreach (var model in PsychotestSchedules.Where(x => DateOnly.FromDateTime(x.ScheduleStartDate!.Value.DateTime).ToString("dd.MM.yyyy").Equals(args.ItemData.ScheduleDate)).ToList())
        {
            SessionTestModels.Add(new SessionTestModel
            {
                ScheduleId = model.Id,
                Session = $"{model.Session} : {model.Session1Start.ToShortTimeString()} - {model.Session1End.ToShortTimeString()}"
            });
        }

        StateHasChanged();
    }

    public void OnChangePsychotestScheduleSessionAsync(ChangeEventArgs<Guid, SessionTestModel> args)
    {
        if (args != null)
        {
            SelectionPsychotestScheduleId = args.ItemData.ScheduleId;
            StateHasChanged();
        }
    }
}
