using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Psmp.Base.Payments.Enums;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Common.Components;

public partial class DialogValidation
{
    [Parameter]
    public bool SecondVisibility { get; set; } = false;
    [Parameter]
    public EventCallback<string> SecondDialogClick { get; set; }
    [Parameter]
    public EventCallback SecondDialogClickClose { get; set; }
    [Parameter]
    public PaymentStatus CondStatus { get; set; } = PaymentStatus.NewRequest;
    [Parameter]
    public string Title { get; set; } = string.Empty;
    [Parameter]
    public string SubTitle { get; set; } = string.Empty;
    [Parameter]
    public string Comment { get; set; } = string.Empty;

    private bool _isSubmitDisabled = true;

    private void OnCommentInput(TextAreaInputEventArgs args)
    {
        Comment = args.Value ?? string.Empty;
        _isSubmitDisabled = string.IsNullOrWhiteSpace(Comment);
    }
    private async Task SubmitWithValidation()
    {
        // temp: for showing status
        var statusFormatted = Regex.Replace(CondStatus.ToString(), "([a-z])([A-Z])", "$1 $2").ToUpper();
        var commentWithStatus = string.Concat($"[{statusFormatted}] ", Comment);

        if (_isSubmitDisabled && !string.IsNullOrWhiteSpace(Comment))
        {
            return;
        }

        await SecondDialogClick.InvokeAsync(commentWithStatus);
        //await SecondDialogClick.InvokeAsync(Comment);
    }
    private record StatusMeta(string Icon, MudBlazor.Color Color, string SubmitContent, string SubmitCss, string SubmitIconCss, string CancelCss);

    private static readonly IReadOnlyDictionary<PaymentStatus, StatusMeta> _statusMap = new Dictionary<PaymentStatus, StatusMeta>
    {
        { PaymentStatus.Approved,               new StatusMeta(Icons.Material.Filled.Check,       MudBlazor.Color.Success, "Approve",      "e-info",    "e-icons e-check",     "e-danger") },
        { PaymentStatus.RequestPreparation,     new StatusMeta(Icons.Material.Filled.Check,       MudBlazor.Color.Success, "Approve",      "e-info",    "e-icons e-check",     "e-danger") },
        { PaymentStatus.NeedRevision,           new StatusMeta(Icons.Material.Filled.Warning,     MudBlazor.Color.Warning, "Need Revision", "e-warning", "e-icons e-warning icon-white", "e-danger") },
        { PaymentStatus.Pending,                new StatusMeta(Icons.Material.Filled.Schedule,    MudBlazor.Color.Default, "Pending",      "e-grey",    "e-icons e-clock",     "e-danger") },
        { PaymentStatus.Rejected,               new StatusMeta(Icons.Material.Filled.Close,       MudBlazor.Color.Error,   "Reject",       "e-danger",  "e-icons e-close",     "e-default") },
        // fallback untuk status lain
        { PaymentStatus.NewRequest,             new StatusMeta(Icons.Material.Filled.Info,        MudBlazor.Color.Default, "Submit",       "e-info",    "e-icons e-check",     "e-danger") }
    };

    private StatusMeta GetMeta(PaymentStatus status)
    {
        if (_statusMap.TryGetValue(status, out var meta))
        {
            return meta;
        }

        return _statusMap[PaymentStatus.NewRequest];
    }
    protected override void OnParametersSet()
    {
        if (SecondVisibility)
        {
            _isSubmitDisabled = true;
        }
    }
}
