using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using Psmp.Bsui.Common.Components;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Features.Documents.Components;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Constants;
using Psmp.Shared.Documents.Queries.GetDocuments;

namespace Psmp.Bsui.Features.Documents;

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
        var dialog = _dialogService.Show<DialogDelete>($"{CommonDisplayTextFor.Delete} {DisplayTextFor.Document} {document.FileName}");
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
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
}
