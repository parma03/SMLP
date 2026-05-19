using Microsoft.AspNetCore.Components;
using Smlp.Shared.Payments.Commands;

namespace Smlp.Bsui.Features.Payments.Component;

public partial class DialogEditStatusSap
{
    [Parameter]
    public bool Visibility { get; set; }

    [Parameter]
    public string Title { get; set; } = "Edit Data";

    [Parameter]
    public PaymentOverviewStatusRequest Request { get; set; } = default!;

    [Parameter]
    public EventCallback DialogClose { get; set; }

    [Parameter]
    public EventCallback OnClickCancel { get; set; }

    [Parameter]
    public EventCallback OnClickSave { get; set; }

    [Parameter]
    public bool IsLoading { get; set; } = false;
    private bool IsSaveDisabled => IsLoading || string.IsNullOrWhiteSpace(Request?.MessageResponse);
}
