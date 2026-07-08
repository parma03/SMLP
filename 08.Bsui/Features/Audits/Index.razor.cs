using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Audits.Components;
using Smlp.Shared.Audits.Constants;
using Smlp.Shared.Audits.Queries.ExportAudits;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;

namespace Smlp.Bsui.Features.Audits;

public partial class Index
{
    private ErrorResponse? _error;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private MudTable<GetAuditsAudit> _tableAudits = new();
    private string? _searchKeyword;
    private FilterModel _filterModel = default!;
    private HashSet<GetAuditsAudit> _selectedAudits = new();

    protected override void OnInitialized()
    {
        _filterModel = new();
        _selectedAudits = new();

        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active(DisplayTextFor.Audits)
        };
    }

    private async Task<TableData<GetAuditsAudit>> ReloadTableAudits(TableState state)
    {
        _error = null;

        StateHasChanged();

        var tableData = new TableData<GetAuditsAudit>();

        var request = state.ToPaginatedListRequest<GetAuditsRequest>(_searchKeyword);
        request.From = _filterModel.From;
        request.To = _filterModel.To;

        var response = await _auditService.GetAuditsAsync(request);

        _error = response.Error;

        StateHasChanged();

        if (response.Result is null)
        {
            return tableData;
        }

        return response.Result.ToTableData();
    }

    private async Task ShowDialogFilter()
    {
        var parameters = new DialogParameters
        {
            { nameof(DialogFilter.Model), _filterModel }
        };

        var dialog = _dialogService.Show<DialogFilter>($"{CommonDisplayTextFor.Filter} data {DisplayTextFor.Audits}", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)

            await _tableAudits.ReloadServerData();

        _selectedAudits = new HashSet<GetAuditsAudit>();
    }

    private async Task OnSearch(string keyword)
    {
        _searchKeyword = keyword.Trim();

        await _tableAudits.ReloadServerData();
    }

    private async Task ExportAudits()
    {
        var request = new ExportAuditsRequest
        {
            AuditIds = _selectedAudits.Select(x => x.Id).ToList()
        };

        var response = await _auditService.ExportAuditsAsync(request);

        if (response.Error is not null)
        {
            _snackbar.AddErrors(response.Error.Details);

            return;
        }

        await _jsRuntime.InvokeVoidAsync(
            JavaScriptIdentifierFor.DownloadFile,
            response.Result!.FileName,
            response.Result.ContentType,
            response.Result.Content);
    }
}
