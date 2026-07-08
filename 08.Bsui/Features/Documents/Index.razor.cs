using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Bsui.Common.Components;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Documents.Components;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Constants;
using Smlp.Shared.Documents.Queries.GetDocuments;

namespace Smlp.Bsui.Features.Documents;

public partial class Index
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private ClaimsPrincipal _user = default!;

    private ErrorResponse? _error;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private MudTable<GetDocumentsDocument> _tableDocuments = new();
    private string? _searchKeyword;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;

        SetupBreadcrumb();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _user = (await AuthenticationStateTask).User;
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active(DisplayTextFor.Documents)
        };
    }

    private async Task<TableData<GetDocumentsDocument>> ReloadTableDocuments(TableState state)
    {
        _error = null;

        StateHasChanged();

        var tableData = new TableData<GetDocumentsDocument>();
        var request = state.ToPaginatedListRequest(_searchKeyword);
        var response = await _documentService.GetDocumentsAsync(request);

        _error = response.Error;

        StateHasChanged();

        if (response.Result is null)
        {
            return tableData;
        }

        return response.Result.ToTableData();
    }

    private async Task OnSearch(string keyword)
    {
        _searchKeyword = keyword.Trim();

        await _tableDocuments.ReloadServerData();
    }

    private async Task Refresh()
    {
        await _tableDocuments.ReloadServerData();

        _snackbar.AddSuccess(SuccessMessageFor.Action(DisplayTextFor.Documents, CommonDisplayTextFor.Refreshed, true));
    }

    private async Task ShowDialogAdd()
    {
        var dialog = _dialogService.Show<DialogAdd>($"{CommonDisplayTextFor.Add} {DisplayTextFor.Document}");
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            _snackbar.AddSuccess(SuccessMessageFor.Action(DisplayTextFor.Document, CommonDisplayTextFor.Added));

            await _tableDocuments.ReloadServerData();
        }
    }

    private async Task Download(Guid documentId)
    {
        var response = await _documentService.GetDocumentFileAsync(documentId);

        if (response.Error is not null)
        {
            _snackbar.AddErrors(response.Error.Details);

            return;
        }

        if (response.Result is null)
        {
            _snackbar.AddError($"Failed to retrieve the {DisplayTextFor.Document} {CommonDisplayTextFor.File}.");

            return;
        }

        await _jsRuntime.InvokeVoidAsync(
            JavaScriptIdentifierFor.DownloadFile,
            response.Result.FileName,
            response.Result.ContentType,
            response.Result.Content);
    }

    private async Task ShowDialogDelete(GetDocumentsDocument document)
    {
        var parameters = new DialogParameters
        {
            [nameof(ConfirmDialog.Title)] = $"{CommonDisplayTextFor.Delete} {DisplayTextFor.Document}",
            [nameof(ConfirmDialog.Message)] = $"Hapus dokumen \"{document.FileName}\"? Tindakan ini tidak bisa dibatalkan.",
            [nameof(ConfirmDialog.Icon)] = Icons.Material.Filled.Delete,
            [nameof(ConfirmDialog.ConfirmText)] = "Ya, Hapus",
        };
        var dialog = await _dialogService.ShowAsync<ConfirmDialog>($"{CommonDisplayTextFor.Delete} {DisplayTextFor.Document} {document.FileName}", parameters);
        var dialogResult = await dialog.Result;
        if (dialogResult is null || dialogResult.Canceled)
            return;

        var response = await _documentService.DeleteDocumentAsync(document.Id);

        if (response.Error is not null)
        {
            _snackbar.AddErrors(response.Error.Details);

            return;
        }

        _snackbar.AddSuccess(SuccessMessageFor.Action(DisplayTextFor.Document, document.FileName, CommonDisplayTextFor.Deleted));

        await _tableDocuments.ReloadServerData();
    }
}
