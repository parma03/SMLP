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
    private bool _loading = false;
    private int _totalCount = 0;
    private int _pageNumber = 1;
    private readonly int _pageSize = 10;

    private List<GetCustomerResponse> _customers = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        _loading = true;
        try
        {
            var request = new GetCustomerRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Tier = _filterTier,
                Segment = _filterSegment,
                PageNumber = _pageNumber,
                PageSize = _pageSize,
            };

            var result = await _customerService.GetCustomersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _customers = result.Result!.Items.ToList();
            _totalCount = result.Result.TotalCount;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat data: {ex.Message}");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnSearchChanged(string value)
    {
        _searchText = value;
        _pageNumber = 1;
        await LoadCustomersAsync();
    }

    private async Task OnFilterTierChanged(CustomerTier? value)
    {
        _filterTier = value;
        _pageNumber = 1;
        await LoadCustomersAsync();
    }

    private async Task OnFilterSegmentChanged(CustomerSegment? value)
    {
        _filterSegment = value;
        _pageNumber = 1;
        await LoadCustomersAsync();
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
        await LoadCustomersAsync();
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

            // Header
            var headers = new[] { "Kode Member", "Nama", "No HP", "Email", "Tier", "Segmen", "Total Poin", "Total Belanja (Rp)", "Transaksi Terakhir", "Tanggal Daftar" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#6F4E37");
                ws.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            // Data
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

    private async Task ExportToCsvAsync()
    {
        try
        {
            // Load all data for export (up to 5000)
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
        CustomerTier.Bronze => "Bronze",
        CustomerTier.Silver => "Silver",
        CustomerTier.Gold => "Gold",
        CustomerTier.Platinum => "Platinum",
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
