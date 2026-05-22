using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Base.Customer.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Queries;

namespace Smlp.Bsui.Features.Customers;

public partial class Index
{
    private string _searchText = string.Empty;
    private CustomerTier? _filterTier;
    private CustomerSegment? _filterSegment;
    private MudTable<GetCustomerResponse>? _table;
    private int _totalCount = 0;

    private async Task<TableData<GetCustomerResponse>> ServerReload(TableState state, CancellationToken ct)
    {
        try
        {
            var request = new GetCustomerRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Tier = _filterTier,
                Segment = _filterSegment,
                PageNumber = state.Page + 1,
                PageSize = state.PageSize,
            };

            var result = await _customerService.GetCustomersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return new TableData<GetCustomerResponse> { TotalItems = 0, Items = [] };
            }

            _totalCount = result.Result!.TotalCount;
            return new TableData<GetCustomerResponse>
            {
                TotalItems = _totalCount,
                Items = result.Result.Items.ToList()
            };
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat data: {ex.Message}");
            return new TableData<GetCustomerResponse> { TotalItems = 0, Items = [] };
        }
    }

    private async Task OnSearchChanged(string value)
    {
        _searchText = value;
        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task OnFilterTierChanged(CustomerTier? value)
    {
        _filterTier = value;
        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task OnFilterSegmentChanged(CustomerSegment? value)
    {
        _filterSegment = value;
        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task OnDeleteAsync(GetCustomerResponse customer)
    {
        var result = await _customerService.DeleteCustomerAsync(customer.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Pelanggan {customer.FullName} berhasil dihapus.");
        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task ExportToExcelAsync()
    {
        try
        {
            var request = new GetCustomerRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Tier = _filterTier,
                Segment = _filterSegment,
                PageNumber = 1,
                PageSize = 5000,
            };
            var result = await _customerService.GetCustomersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var rows = result.Result!.Items;
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Pelanggan");

            var headers = new[] { "Kode Member", "Nama", "No HP", "Email", "Tier", "Segmen", "Total Poin", "Total Belanja (Rp)", "Transaksi Terakhir", "Tanggal Daftar" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#6F4E37");
                ws.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            int row = 2;
            foreach (var c in rows)
            {
                ws.Cell(row, 1).Value = c.MemberCode;
                ws.Cell(row, 2).Value = c.FullName;
                ws.Cell(row, 3).Value = c.PhoneNumber;
                ws.Cell(row, 4).Value = c.EmailAddress ?? "";
                ws.Cell(row, 5).Value = GetTierLabel(c.Tier);
                ws.Cell(row, 6).Value = GetSegmentLabel(c.Segment);
                ws.Cell(row, 7).Value = c.TotalPoints;
                ws.Cell(row, 8).Value = (double)c.TotalSpend;
                ws.Cell(row, 9).Value = c.LastTransactionDate?.ToString("yyyy-MM-dd") ?? "";
                ws.Cell(row, 10).Value = c.CreatedAt.ToString("yyyy-MM-dd");
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var base64 = Convert.ToBase64String(stream.ToArray());
            var fileName = $"Pelanggan_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", base64);
            _snackbar.AddSuccess($"Export Excel berhasil: {rows.Count()} data pelanggan.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal export Excel: {ex.Message}");
        }
    }

    private async Task ExportToPdfAsync()
    {
        try
        {
            var request = new GetCustomerRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Tier = _filterTier,
                Segment = _filterSegment,
                PageNumber = 1,
                PageSize = 5000,
            };
            var result = await _customerService.GetCustomersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var rows = result.Result!.Items;
            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html><head><meta charset='UTF-8'/>
<style>
body { font-family: Arial, sans-serif; font-size: 11px; margin: 20px; color: #333; }
h1 { color: #6F4E37; font-size: 18px; margin-bottom: 4px; }
.subtitle { color: #795548; font-size: 10px; margin-bottom: 16px; }
table { border-collapse: collapse; width: 100%; }
th { background: #6F4E37; color: white; padding: 7px 8px; text-align: left; font-size: 10px; }
td { padding: 5px 8px; border-bottom: 1px solid #F0E9E3; font-size: 10px; }
tr:nth-child(even) td { background: #FFF8F0; }
.tier-bronze { color: #A0522D; font-weight: bold; }
.tier-silver { color: #607D8B; font-weight: bold; }
.tier-gold { color: #C8860A; font-weight: bold; }
.tier-platinum { color: #6F4E37; font-weight: bold; }
.points { color: #C8860A; font-weight: 600; }
.amount { color: #2E7D32; font-weight: 600; }
.footer { margin-top: 20px; font-size: 9px; color: #999; }
</style></head><body>
<h1>📋 Laporan Data Pelanggan</h1>
<div class='subtitle'>Dicetak: ");
            sb.Append(DateTime.Now.ToString("dd MMMM yyyy HH:mm"));
            sb.Append($" &nbsp;|&nbsp; Total: {rows.Count()} pelanggan</div>");
            sb.Append(@"<table><thead><tr>
<th>Kode Member</th><th>Nama Pelanggan</th><th>No. HP</th><th>Tier</th><th>Segmen</th>
<th style='text-align:right'>Total Poin</th><th style='text-align:right'>Total Belanja</th><th>Transaksi Terakhir</th>
</tr></thead><tbody>");

            foreach (var c in rows)
            {
                var tierCss = c.Tier switch
                {
                    Smlp.Base.Customer.Enums.CustomerTier.Bronze => "tier-bronze",
                    Smlp.Base.Customer.Enums.CustomerTier.Silver => "tier-silver",
                    Smlp.Base.Customer.Enums.CustomerTier.Gold => "tier-gold",
                    Smlp.Base.Customer.Enums.CustomerTier.Platinum => "tier-platinum",
                    _ => ""
                };
                sb.Append($"<tr><td><b>{System.Net.WebUtility.HtmlEncode(c.MemberCode)}</b></td>");
                sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(c.FullName)}</td>");
                sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(c.PhoneNumber)}</td>");
                sb.Append($"<td class='{tierCss}'>{GetTierLabel(c.Tier)}</td>");
                sb.Append($"<td>{GetSegmentLabel(c.Segment)}</td>");
                sb.Append($"<td style='text-align:right' class='points'>⭐ {c.TotalPoints:N0}</td>");
                sb.Append($"<td style='text-align:right' class='amount'>Rp {c.TotalSpend:N0}</td>");
                sb.Append($"<td>{c.LastTransactionDate?.ToString("dd MMM yyyy") ?? "-"}</td></tr>");
            }

            sb.Append("</tbody></table>");
            sb.Append($"<div class='footer'>SMLP — Sistem Manajemen Loyalitas Pelanggan &copy; {DateTime.Now.Year}</div>");
            sb.Append("</body></html>");

            var htmlBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var base64 = Convert.ToBase64String(htmlBytes);
            var fileName = $"Pelanggan_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName, "text/html", base64);
            _snackbar.AddSuccess($"Export HTML/PDF berhasil: {rows.Count()} data. Buka file lalu Print → Save as PDF.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal export PDF: {ex.Message}");
        }
    }

    private async Task ExportToCsvAsync()
    {
        try
        {
            var request = new GetCustomerRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Tier = _filterTier,
                Segment = _filterSegment,
                PageNumber = 1,
                PageSize = 5000,
            };
            var result = await _customerService.GetCustomersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var rows = result.Result!.Items;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Kode Member,Nama,No HP,Email,Tier,Segmen,Total Poin,Total Belanja,Transaksi Terakhir,Tanggal Daftar");

            foreach (var c in rows)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(c.MemberCode),
                    CsvEscape(c.FullName),
                    CsvEscape(c.PhoneNumber),
                    CsvEscape(c.EmailAddress ?? ""),
                    c.Tier.ToString(),
                    GetSegmentLabel(c.Segment),
                    c.TotalPoints.ToString(),
                    c.TotalSpend.ToString("F2"),
                    c.LastTransactionDate?.ToString("yyyy-MM-dd") ?? "",
                    c.CreatedAt.ToString("yyyy-MM-dd")
                ));
            }

            var csvBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var base64 = Convert.ToBase64String(csvBytes);
            var fileName = $"Pelanggan_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName, "text/csv", base64);
            _snackbar.AddSuccess($"Export berhasil: {rows.Count()} data pelanggan.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal export: {ex.Message}");
        }
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private Color GetTierColor(CustomerTier tier) => tier switch
    {
        CustomerTier.Gold => Color.Warning,
        CustomerTier.Silver => Color.Default,
        CustomerTier.Platinum => Color.Primary,
        _ => Color.Default
    };

    private string GetTierLabel(CustomerTier tier) => tier switch
    {
        CustomerTier.Bronze => "🥉 Bronze",
        CustomerTier.Silver => "🥈 Silver",
        CustomerTier.Gold => "🥇 Gold",
        CustomerTier.Platinum => "💎 Platinum",
        _ => tier.ToString()
    };

    private Color GetSegmentColor(CustomerSegment segment) => segment switch
    {
        CustomerSegment.Loyal => Color.Success,
        CustomerSegment.Active => Color.Info,
        CustomerSegment.AtRisk => Color.Warning,
        CustomerSegment.Inactive => Color.Error,
        _ => Color.Default
    };

    private string GetSegmentLabel(CustomerSegment segment) => segment switch
    {
        CustomerSegment.New => "Baru",
        CustomerSegment.Active => "Aktif",
        CustomerSegment.Loyal => "Setia",
        CustomerSegment.AtRisk => "Berisiko",
        CustomerSegment.Inactive => "Tidak Aktif",
        _ => segment.ToString()
    };
}
