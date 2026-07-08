using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Analytics.Queries.ExportAnalytics;
using Smlp.Shared.Analytics.Queries.GetRevenueChart;

namespace Smlp.Application.Analytics.Queries;

public class ExportAnalyticsQuery : IRequest<ExportAnalyticsResponse>
{
    public ExportAnalyticsRequest Request { get; set; } = new();
}

public class ExportAnalyticsQueryHandler : IRequestHandler<ExportAnalyticsQuery, ExportAnalyticsResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IMediator _mediator;

    public ExportAnalyticsQueryHandler(ISmlpDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<ExportAnalyticsResponse> Handle(ExportAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        using var workbook = new XLWorkbook();
        string fileName;

        switch (req.Type.ToLower())
        {
            case "revenue":
                await BuildRevenueSheet(workbook, req, cancellationToken);
                fileName = $"Analytics_Revenue_{req.Year}{(req.Month > 0 ? $"_{req.Month:D2}" : "")}.xlsx";
                break;

            case "loyalty":
                await BuildLoyaltySheet(workbook, cancellationToken);
                fileName = $"Analytics_Loyalty_{DateTime.Now:yyyyMMdd}.xlsx";
                break;

            case "customer-insights":
                await BuildCustomerInsightsSheet(workbook, cancellationToken);
                fileName = $"Analytics_CustomerInsights_{DateTime.Now:yyyyMMdd}.xlsx";
                break;

            case "dashboard":
            default:
                await BuildDashboardSheet(workbook, cancellationToken);
                fileName = $"Analytics_Dashboard_{DateTime.Now:yyyyMMdd}.xlsx";
                break;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ExportAnalyticsResponse
        {
            FileName = fileName,
            Content = stream.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }

    // ── Revenue Sheet ────────────────────────────────────────────────────────
    private async Task BuildRevenueSheet(XLWorkbook wb, ExportAnalyticsRequest req, CancellationToken ct)
    {
        var revenue = await _mediator.Send(new GetRevenueChartQuery { Request = new GetRevenueChartRequest { Year = req.Year, Month = req.Month } }, ct);

        var ws = wb.Worksheets.Add("Revenue Bulanan");
        ApplyHeader(ws, new[] { "Bulan", "Revenue (Rp)", "Jumlah Transaksi", "Poin Diterbitkan" });

        var row = 2;
        foreach (var m in revenue.MonthlyRevenue)
        {
            ws.Cell(row, 1).Value = m.Label;
            ws.Cell(row, 2).Value = (double)m.Amount;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 3).Value = m.Transactions;
            ws.Cell(row, 4).Value = m.PointsEarned;
            row++;
        }

        // Total row
        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = (double)revenue.MonthlyRevenue.Sum(m => m.Amount);
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = revenue.MonthlyRevenue.Sum(m => m.Transactions);
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = revenue.MonthlyRevenue.Sum(m => m.PointsEarned);
        ws.Cell(row, 4).Style.Font.Bold = true;

        if (revenue.DailyRevenue.Any())
        {
            var ws2 = wb.Worksheets.Add("Revenue Harian");
            ApplyHeader(ws2, new[] { "Hari", "Revenue (Rp)", "Jumlah Transaksi", "Poin Diterbitkan" });
            var row2 = 2;
            foreach (var d in revenue.DailyRevenue)
            {
                ws2.Cell(row2, 1).Value = d.Label;
                ws2.Cell(row2, 2).Value = (double)d.Amount;
                ws2.Cell(row2, 2).Style.NumberFormat.Format = "#,##0";
                ws2.Cell(row2, 3).Value = d.Transactions;
                ws2.Cell(row2, 4).Value = d.PointsEarned;
                row2++;
            }

            ws2.Columns(1, 4).AdjustToContents();
        }

        // Payment methods sheet
        if (revenue.PaymentMethods.Any())
        {
            var ws3 = wb.Worksheets.Add("Metode Pembayaran");
            ApplyHeader(ws3, new[] { "Metode", "Total Revenue (Rp)", "Jumlah Transaksi" });
            var row3 = 2;
            foreach (var p in revenue.PaymentMethods)
            {
                ws3.Cell(row3, 1).Value = p.Method;
                ws3.Cell(row3, 2).Value = (double)p.Amount;
                ws3.Cell(row3, 2).Style.NumberFormat.Format = "#,##0";
                ws3.Cell(row3, 3).Value = p.Count;
                row3++;
            }

            ws3.Columns(1, 3).AdjustToContents();
        }

        ws.Columns(1, 4).AdjustToContents();
    }

    // ── Loyalty Sheet ────────────────────────────────────────────────────────
    private async Task BuildLoyaltySheet(XLWorkbook wb, CancellationToken ct)
    {
        var loyalty = await _mediator.Send(new GetLoyaltyAnalysisQuery(), ct);

        var ws = wb.Worksheets.Add("Ekonomi Poin");
        ApplyHeader(ws, new[] { "Metrik", "Nilai" });

        var row = 2;
        void AddRow(string label, object val)
        { ws.Cell(row, 1).Value = label; ws.Cell(row, 2).Value = val?.ToString() ?? ""; row++; }

        AddRow("Total Poin Diterbitkan", loyalty.TotalPointsIssued.ToString("N0"));
        AddRow("Total Poin Digunakan", loyalty.TotalPointsRedeemed.ToString("N0"));
        AddRow("Total Poin Kadaluarsa", loyalty.TotalPointsExpired.ToString("N0"));
        AddRow("Poin Beredar", loyalty.TotalPointsCirculating.ToString("N0"));
        AddRow("Redemption Rate", $"{loyalty.RedemptionRate:F1}%");
        AddRow("Manual Penambahan", loyalty.TotalManualAdditions.ToString("N0"));
        AddRow("Manual Pengurangan", loyalty.TotalManualDeductions.ToString("N0"));

        ws.Columns(1, 2).AdjustToContents();

        // Reward performance
        if (loyalty.RewardPerformance.Any())
        {
            var ws2 = wb.Worksheets.Add("Performa Reward");
            ApplyHeader(ws2, new[] { "Reward", "Total Redeem", "Total Poin Digunakan" });
            var r2 = 2;
            foreach (var rp in loyalty.RewardPerformance)
            {
                ws2.Cell(r2, 1).Value = rp.RewardName;
                ws2.Cell(r2, 2).Value = rp.RedeemCount;
                ws2.Cell(r2, 3).Value = rp.TotalPointsUsed;
                r2++;
            }

            ws2.Columns(1, 3).AdjustToContents();
        }
    }

    // ── Customer Insights Sheet ─────────────────────────────────────────────
    private async Task BuildCustomerInsightsSheet(XLWorkbook wb, CancellationToken ct)
    {
        var insights = await _mediator.Send(new GetCustomerInsightsQuery(), ct);

        // Top customers
        var ws1 = wb.Worksheets.Add("Top Pelanggan");
        ApplyHeader(ws1, new[] { "No", "Nama", "Kode Member", "Total Belanja (Rp)", "Total Transaksi", "Poin Aktif", "Tier" });
        var r1 = 2;
        foreach (var (c, i) in insights.TopSpenders.Select((c, i) => (c, i + 1)))
        {
            ws1.Cell(r1, 1).Value = i;
            ws1.Cell(r1, 2).Value = c.FullName;
            ws1.Cell(r1, 3).Value = c.MemberCode;
            ws1.Cell(r1, 4).Value = (double)c.TotalSpend;
            ws1.Cell(r1, 4).Style.NumberFormat.Format = "#,##0";
            ws1.Cell(r1, 5).Value = c.TransactionCount;
            ws1.Cell(r1, 6).Value = c.TotalPoints;
            ws1.Cell(r1, 7).Value = c.Tier;
            r1++;
        }

        ws1.Columns(1, 7).AdjustToContents();

        // Churn risk
        if (insights.ChurnRiskCustomers.Any())
        {
            var ws2 = wb.Worksheets.Add("Risiko Churn");
            ApplyHeader(ws2, new[] { "Nama", "Kode Member", "Hari Terakhir Transaksi", "Tier" });
            var r2 = 2;
            foreach (var c in insights.ChurnRiskCustomers)
            {
                ws2.Cell(r2, 1).Value = c.FullName;
                ws2.Cell(r2, 2).Value = c.MemberCode;
                ws2.Cell(r2, 3).Value = c.DaysSinceLastPurchase;
                ws2.Cell(r2, 4).Value = c.Tier;
                r2++;
            }

            ws2.Columns(1, 4).AdjustToContents();
        }
    }

    // ── Dashboard Sheet ─────────────────────────────────────────────────────
    private async Task BuildDashboardSheet(XLWorkbook wb, CancellationToken ct)
    {
        var summary = await _mediator.Send(new GetDashboardSummaryQuery(), ct);

        var ws = wb.Worksheets.Add("Ringkasan Dashboard");
        ApplyHeader(ws, new[] { "Metrik", "Nilai" });

        var row = 2;
        void AddRow(string label, string val)
        { ws.Cell(row, 1).Value = label; ws.Cell(row, 2).Value = val; row++; }

        AddRow("Total Pelanggan", summary.TotalCustomers.ToString("N0"));
        AddRow("Pelanggan Baru Bulan Ini", summary.NewCustomersThisMonth.ToString("N0"));
        AddRow("Pelanggan Aktif", summary.ActiveCustomers.ToString("N0"));
        AddRow("Pelanggan Tidak Aktif", summary.InactiveCustomers.ToString("N0"));
        AddRow("Revenue Bulan Ini (Rp)", summary.TotalRevenueThisMonth.ToString("N0"));
        AddRow("Revenue Bulan Lalu (Rp)", summary.TotalRevenueLastMonth.ToString("N0"));
        AddRow("Pertumbuhan Revenue", $"{summary.RevenueGrowthPercent:F1}%");
        AddRow("Transaksi Hari Ini", summary.TotalTransactionsToday.ToString("N0"));
        AddRow("Transaksi Bulan Ini", summary.TotalTransactionsThisMonth.ToString("N0"));
        AddRow("Poin Beredar", summary.TotalPointsCirculating.ToString("N0"));
        AddRow("Poin Digunakan", summary.TotalPointsRedeemed.ToString("N0"));
        AddRow("Redeem Pending", summary.TotalPendingRedeems.ToString("N0"));
        AddRow("Pelanggan Bronze", summary.BronzeCount.ToString("N0"));
        AddRow("Pelanggan Silver", summary.SilverCount.ToString("N0"));
        AddRow("Pelanggan Gold", summary.GoldCount.ToString("N0"));
        AddRow("Pelanggan Platinum", summary.PlatinumCount.ToString("N0"));
        AddRow("Tanggal Export", DateTime.Now.ToString("dd MMMM yyyy HH:mm"));

        ws.Columns(1, 2).AdjustToContents();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static void ApplyHeader(IXLWorksheet ws, string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }
}
