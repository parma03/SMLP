using Microsoft.JSInterop;
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
    private bool _isExporting = false;
    private string? _exportingType = null;
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

    private async Task ExportReportAsync(string type)
    {
        if (_isExporting) return;
        _isExporting = true;
        _exportingType = type;

        try
        {
            var dateFrom = _reportFrom.HasValue ? new DateTimeOffset(_reportFrom.Value) : (DateTimeOffset?)null;
            var dateTo = _reportTo.HasValue ? new DateTimeOffset(_reportTo.Value.AddDays(1)) : (DateTimeOffset?)null;

            byte[] fileBytes;
            string fileName;
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            switch (type)
            {
                case "transactions":
                    var txResult = await _reportService.GetTransactionReportAsync(new GetTransactionReportRequest
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        PageSize = 5000
                    });
                    if (txResult.Error is not null) { _snackbar.AddError(txResult.Error.ToDetail()); return; }
                    fileBytes = BuildTransactionExcel(txResult.Result!.Items.ToList());
                    fileName = $"Laporan_Transaksi_{DateTime.Now:yyyyMMdd}.xlsx";
                    break;

                case "redeems":
                    var rdResult = await _reportService.GetRedeemReportAsync(new GetRedeemReportRequest
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        PageSize = 5000
                    });
                    if (rdResult.Error is not null) { _snackbar.AddError(rdResult.Error.ToDetail()); return; }
                    fileBytes = BuildRedeemExcel(rdResult.Result!.Items.ToList());
                    fileName = $"Laporan_Redeem_{DateTime.Now:yyyyMMdd}.xlsx";
                    break;

                case "points":
                    var ptResult = await _reportService.GetPointReportAsync(new GetPointReportRequest
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        PageSize = 5000
                    });
                    if (ptResult.Error is not null) { _snackbar.AddError(ptResult.Error.ToDetail()); return; }
                    fileBytes = BuildPointExcel(ptResult.Result!.Items.ToList());
                    fileName = $"Laporan_Poin_{DateTime.Now:yyyyMMdd}.xlsx";
                    break;

                default:
                    _snackbar.AddError("Jenis laporan tidak dikenal.");
                    return;
            }

            var base64 = Convert.ToBase64String(fileBytes);
            await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName, contentType, base64);
            _snackbar.AddSuccess($"Export '{GetReportTitle()}' berhasil diunduh.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal export: {ex.Message}");
        }
        finally
        {
            _isExporting = false;
            _exportingType = null;
        }
    }

    // --- Excel builders using ClosedXML-style byte array via OpenXml placeholder ---
    // These build a simple CSV-like Excel using basic tab-separated approach.
    // For production use, inject ClosedXML or EPPlus in the client project.

    private static byte[] BuildTransactionExcel(List<GetTransactionReportResponse> rows)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("No. Transaksi,Pelanggan,Kode Member,Tanggal,Total Amount,Poin Didapat,Metode Bayar,Status,Tier");
        foreach (var r in rows)
            csv.AppendLine($"{r.TransactionCode},{r.CustomerName},{r.MemberCode},{r.TransactionDate.ToLocalTime():dd/MM/yyyy HH:mm},{r.TotalAmount:N0},{r.PointsEarned},{r.PaymentMethod},{r.Status},{r.Tier}");
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static byte[] BuildRedeemExcel(List<GetRedeemReportResponse> rows)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Kode Redeem,Pelanggan,Kode Member,Reward,Tanggal Redeem,Poin Digunakan,Status,Diverifikasi Oleh");
        foreach (var r in rows)
            csv.AppendLine($"{r.RedeemCode},{r.CustomerName},{r.MemberCode},{r.RewardName},{r.RedeemDate.ToLocalTime():dd/MM/yyyy HH:mm},{r.PointsUsed},{r.Status},{r.VerifiedBy}");
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static byte[] BuildPointExcel(List<GetPointReportResponse> rows)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Pelanggan,Kode Member,Jenis Aktivitas,Poin,Deskripsi,Tanggal,Saldo Setelah");
        foreach (var r in rows)
            csv.AppendLine($"{r.CustomerName},{r.MemberCode},{r.ActivityType},{r.Points},{r.Description},{r.ActivityDate.ToLocalTime():dd/MM/yyyy},{r.BalanceAfter}");
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
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
