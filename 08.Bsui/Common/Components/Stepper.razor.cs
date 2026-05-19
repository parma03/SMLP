using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Common.Components;

public partial class Stepper
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private void Refresh()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}
