using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Reports.Queries.GetTransactionReport;
using Smlp.Shared.Reports.Queries.GetRedeemReport;
using Smlp.Shared.Reports.Queries.GetPointReport;

namespace Smlp.Bsui.Features.Reports;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private string _reportType = "transactions";
    private DateTime? _reportFrom = DateTime.Today.AddMonths(-1);
    private DateTime? _reportTo = DateTime.Today;
    private bool _isGenerating = false;
    private bool _hasData = false;

    // Data lists
    private List<GetTransactionReportResponse> _transactionData = new();
    private List<GetRedeemReportResponse> _redeemData = new();
    private List<GetPointReportResponse> _pointData = new();

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active("Laporan"),
        };
    }

    private async Task GenerateReportAsync()
    {
        _isGenerating = true;
        _hasData = false;
        _transactionData = new();
        _redeemData = new();
        _pointData = new();

        try
        {
            var dateFrom = _reportFrom.HasValue ? new DateTimeOffset(_reportFrom.Value) : (DateTimeOffset?)null;
            var dateTo = _reportTo.HasValue ? new DateTimeOffset(_reportTo.Value.AddDays(1)) : (DateTimeOffset?)null;

            switch (_reportType)
            {
                case "transactions":
                    var txResult = await _reportService.GetTransactionReportAsync(new GetTransactionReportRequest
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        PageSize = 200
                    });
                    if (txResult.Error is not null)
                    {
                        _snackbar.AddError(txResult.Error.ToDetail());
                        return;
                    }

                    _transactionData = txResult.Result!.Items.ToList();
                    break;

                case "redeems":
                    var rdResult = await _reportService.GetRedeemReportAsync(new GetRedeemReportRequest
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        PageSize = 200
                    });
                    if (rdResult.Error is not null)
                    {
                        _snackbar.AddError(rdResult.Error.ToDetail());
                        return;
                    }

                    _redeemData = rdResult.Result!.Items.ToList();
                    break;

                case "points":
                    var ptResult = await _reportService.GetPointReportAsync(new GetPointReportRequest
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        PageSize = 200
                    });
                    if (ptResult.Error is not null)
                    {
                        _snackbar.AddError(ptResult.Error.ToDetail());
                        return;
                    }

                    _pointData = ptResult.Result!.Items.ToList();
                    break;
            }

            _hasData = true;
            _snackbar.AddSuccess("Laporan berhasil dimuat.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal membuat laporan: {ex.Message}");
        }
        finally
        {
            _isGenerating = false;
        }
    }

    private string GetReportTitle() => _reportType switch
    {
        "transactions" => "Laporan Transaksi",
        "redeems" => "Laporan Penukaran Reward",
        "points" => "Laporan Poin",
        _ => "Laporan"
    };

    private int GetRowCount() => _reportType switch
    {
        "transactions" => _transactionData.Count,
        "redeems" => _redeemData.Count,
        "points" => _pointData.Count,
        _ => 0
    };
}
