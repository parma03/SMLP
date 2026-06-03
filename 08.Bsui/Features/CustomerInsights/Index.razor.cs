using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Analytics.Queries.GetCustomerInsights;
using System.Security.Claims;

namespace Smlp.Bsui.Features.CustomerInsights;

public partial class Index
{
    [Inject] private IJSRuntime _js { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private bool _isExporting = false;

    private GetCustomerInsightsResponse? _insights;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
        await LoadData();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active("Customer Insights"),
        };
    }

    private async Task LoadData()
    {
        _isLoading = true;
        try
        {
            var result = await _analyticsService.GetCustomerInsightsAsync();
            if (result.Result is not null)
                _insights = result.Result;
            else if (result.Error is not null)
                _snackbar.AddError(result.Error.ToDetail());
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat customer insights: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    // ---- Donut chart data ----
    private double[] DonutData => _insights?.RfmSegments?
        .Where(s => s.Count > 0)
        .Select(s => (double)s.Count)
        .ToArray() ?? Array.Empty<double>();

    private string[] DonutLabels => _insights?.RfmSegments?
        .Where(s => s.Count > 0)
        .Select(s => s.Label)
        .ToArray() ?? Array.Empty<string>();

    private Color GetMudColor(string colorStr) => colorStr switch
    {
        "success" => Color.Success,
        "info" => Color.Info,
        "warning" => Color.Warning,
        "error" => Color.Error,
        "primary" => Color.Primary,
        _ => Color.Default,
    };

    private double RetentionRate => (double)(_insights?.RetentionRate ?? 0);
    private int TotalSegmented => _insights?.RfmSegments?.Sum(s => s.Count) ?? 0;

    // ---- Export Segmen ke Excel ----
    private async Task ExportSegmentAsync()
    {
        if (_insights is null) return;
        _isExporting = true;
        try
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RFM Segmentasi");

            // Header
            ws.Cell(1, 1).Value = "Segmen";
            ws.Cell(1, 2).Value = "Label";
            ws.Cell(1, 3).Value = "Jumlah Pelanggan";
            ws.Cell(1, 4).Value = "Persentase (%)";
            ws.Cell(1, 5).Value = "Deskripsi";
            ws.Cell(1, 6).Value = "Rekomendasi Aksi";

            var headerRow = ws.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
            headerRow.Style.Font.FontColor = XLColor.White;

            var total = TotalSegmented;
            for (int i = 0; i < _insights.RfmSegments.Count; i++)
            {
                var seg = _insights.RfmSegments[i];
                var row = i + 2;
                ws.Cell(row, 1).Value = seg.Segment;
                ws.Cell(row, 2).Value = seg.Label;
                ws.Cell(row, 3).Value = seg.Count;
                ws.Cell(row, 4).Value = total > 0 ? Math.Round((double)seg.Count / total * 100, 2) : 0;
                ws.Cell(row, 5).Value = seg.Description;
                ws.Cell(row, 6).Value = seg.ActionRecommendation;
            }

            // Summary sheet
            var wsSummary = wb.Worksheets.Add("Ringkasan");
            wsSummary.Cell(1, 1).Value = "Metrik";
            wsSummary.Cell(1, 2).Value = "Nilai";
            wsSummary.Row(1).Style.Font.Bold = true;
            wsSummary.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
            wsSummary.Row(1).Style.Font.FontColor = XLColor.White;

            var summaryData = new[]
            {
                ("Total Pelanggan", _insights.TotalCustomers.ToString("N0")),
                ("Avg. Total Belanja", $"Rp {_insights.AvgTransactionValue:N0}"),
                ("Avg. Poin", _insights.AvgPointsPerCustomer.ToString("N0")),
                ("Retention Rate", $"{_insights.RetentionRate:F1}%"),
                ("Churn Risk", _insights.ChurnRiskCustomers.Count.ToString("N0")),
                ("Tanggal Export", DateTime.Now.ToString("dd/MM/yyyy HH:mm")),
            };

            for (int i = 0; i < summaryData.Length; i++)
            {
                wsSummary.Cell(i + 2, 1).Value = summaryData[i].Item1;
                wsSummary.Cell(i + 2, 2).Value = summaryData[i].Item2;
            }

            ws.Columns().AdjustToContents();
            wsSummary.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();
            var fileName = $"RFM_Segmentasi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var base64 = Convert.ToBase64String(bytes);
            await _js.InvokeVoidAsync("downloadFileFromBase64", fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", base64);

            _snackbar.AddSuccess("Ekspor segmentasi RFM berhasil!");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal mengekspor: {ex.Message}");
        }
        finally
        {
            _isExporting = false;
        }
    }

    // ── Churn Prediction (Rule-Based Score) ───────────────────────────────────
    /// <summary>
    /// Rule-based churn risk score 0–100.
    /// Rules: days absent (max 50), low points (max 30), bronze tier (max 20).
    /// </summary>
    private static int ComputeChurnScore(ChurnRiskItem item)
    {
        var score = 0;

        // Days absent contribution (max 50 pts)
        score += item.DaysSinceLastPurchase switch
        {
            >= 120 => 50,
            >= 90  => 40,
            >= 60  => 28,
            _      => 15
        };

        // Low loyalty points = weaker retention bond (max 30 pts)
        score += item.TotalPoints switch
        {
            0      => 30,
            <= 100 => 20,
            <= 500 => 10,
            _      => 0
        };

        // Lower tier = less retention incentive (max 20 pts)
        score += item.Tier?.ToLower() switch
        {
            "bronze"   => 20,
            "silver"   => 10,
            "gold"     => 5,
            "platinum" => 0,
            _          => 15
        };

        return Math.Min(score, 100);
    }
}
