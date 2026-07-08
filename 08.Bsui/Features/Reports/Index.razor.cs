using ClosedXML.Excel;
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
    private static byte[] BuildTransactionExcel(List<GetTransactionReportResponse> rows)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Transaksi");

        // Header row
        string[] headers = { "No. Transaksi", "Pelanggan", "Kode Member", "Tanggal", "Total Amount", "Poin Didapat", "Metode Bayar", "Status", "Tier" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = r.TransactionCode;
            ws.Cell(row, 2).Value = r.CustomerName;
            ws.Cell(row, 3).Value = r.MemberCode;
            ws.Cell(row, 4).Value = r.TransactionDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 5).Value = r.TotalAmount;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 6).Value = r.PointsEarned;
            ws.Cell(row, 7).Value = r.PaymentMethod;
            ws.Cell(row, 8).Value = r.Status;
            ws.Cell(row, 9).Value = r.Tier;
            if (i % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
        }

        ws.Columns().AdjustToContents();
        using var ms = new System.IO.MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static byte[] BuildRedeemExcel(List<GetRedeemReportResponse> rows)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Redeem");

        string[] headers = { "Kode Redeem", "Pelanggan", "Kode Member", "Reward", "Tanggal Redeem", "Poin Digunakan", "Status", "Diverifikasi Oleh" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = r.RedeemCode;
            ws.Cell(row, 2).Value = r.CustomerName;
            ws.Cell(row, 3).Value = r.MemberCode;
            ws.Cell(row, 4).Value = r.RewardName;
            ws.Cell(row, 5).Value = r.RedeemDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 6).Value = r.PointsUsed;
            ws.Cell(row, 7).Value = r.Status;
            ws.Cell(row, 8).Value = r.VerifiedBy;
            if (i % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
        }

        ws.Columns().AdjustToContents();
        using var ms = new System.IO.MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static byte[] BuildPointExcel(List<GetPointReportResponse> rows)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Poin");

        string[] headers = { "Pelanggan", "Kode Member", "Jenis Aktivitas", "Poin", "Deskripsi", "Tanggal", "Saldo Setelah" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = r.CustomerName;
            ws.Cell(row, 2).Value = r.MemberCode;
            ws.Cell(row, 3).Value = r.ActivityType;
            ws.Cell(row, 4).Value = r.Points;
            ws.Cell(row, 5).Value = r.Description;
            ws.Cell(row, 6).Value = r.ActivityDate.ToLocalTime().ToString("dd/MM/yyyy");
            ws.Cell(row, 7).Value = r.BalanceAfter;
            if (i % 2 == 1)
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F5F5");
        }

        ws.Columns().AdjustToContents();
        using var ms = new System.IO.MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
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
