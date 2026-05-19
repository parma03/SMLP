using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Psmp.Bsui.Common.Components;

public partial class DialogRefresh
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private void Refresh()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}
