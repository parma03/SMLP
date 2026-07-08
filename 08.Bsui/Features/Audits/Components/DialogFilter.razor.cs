using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Bsui.Common.Extensions;

namespace Smlp.Bsui.Features.Audits.Components;

public partial class DialogFilter
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public FilterModel Model { get; set; } = default!;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Apply()
    {
        if (Model.From > Model.To)
        {
            _snackbar.AddError("To must be greater or equals than From");

            return;
        }

        MudDialog.Close(DialogResult.Ok(Model));
    }
}

public class FilterModel
{
    private static readonly DateTime _fromDateDefaultValue = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Utc).AddDays(-7);
    private static readonly DateTime _toDateDefaultValue = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Utc);

    public DateTime? FromDate { get; set; } = _fromDateDefaultValue;
    public TimeSpan? FromTime { get; set; } = new TimeSpan(0, 0, 0);
    public DateTime? ToDate { get; set; } = _toDateDefaultValue;
    public TimeSpan? ToTime { get; set; } = new TimeSpan(23, 59, 59);

    public DateTime From
    {
        get
        {
            var from = FromDate ?? _fromDateDefaultValue;

            if (FromTime.HasValue)
            {
                from = from.AddTicks(FromTime.Value.Ticks);
            }

            return from;
        }
    }

    public DateTime To
    {
        get
        {
            var to = ToDate ?? _toDateDefaultValue;

            if (ToTime.HasValue)
            {
                to = to.AddTicks(ToTime.Value.Ticks);
            }

            return to;
        }
    }
}
