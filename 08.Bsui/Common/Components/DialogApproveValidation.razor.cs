using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Common.Components;
public partial class DialogApproveValidation
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public bool IsSuccess { get; set; }
    [Parameter] public bool CondStatus { get; set; } = true;
    public string? Comment { get; set; }
    private async Task Approve()
    {
        MudDialog?.Close(DialogResult.Ok(Comment));
        //var dialog = await DialogService.ShowAsync<DialogSubmitValidationContinue>("");
        //var result = await dialog.Result;
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
