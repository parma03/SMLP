using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Products.Queries;

namespace Smlp.Bsui.Features.Products;

public partial class Index
{
    private string _searchText = string.Empty;
    private bool? _filterIsActive;
    private MudTable<GetProductResponse>? _table;
    private int _totalCount = 0;

    private async Task<TableData<GetProductResponse>> ServerReload(TableState state, CancellationToken ct)
    {
        try
        {
            var request = new GetProductRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                IsActive = _filterIsActive,
                PageNumber = state.Page + 1,
                PageSize = state.PageSize,
            };

            var result = await _productService.GetProductsAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return new TableData<GetProductResponse> { TotalItems = 0, Items = [] };
            }

            _totalCount = result.Result!.TotalCount;
            return new TableData<GetProductResponse>
            {
                TotalItems = _totalCount,
                Items = result.Result.Items.ToList()
            };
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat data: {ex.Message}");
            return new TableData<GetProductResponse> { TotalItems = 0, Items = [] };
        }
    }

    private async Task OnSearchChanged(string value)
    {
        _searchText = value;
        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task OnFilterActiveChanged(bool? value)
    {
        _filterIsActive = value;
        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task OnDeleteAsync(GetProductResponse product)
    {
        var confirmed = await _dialogService.ShowMessageBox(
            "Konfirmasi Hapus",
            $"Hapus produk \"{product.Name}\"? Tindakan ini tidak bisa dibatalkan.",
            yesText: "Hapus",
            noText: "Batal");

        if (confirmed != true)
            return;

        var result = await _productService.DeleteProductAsync(product.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Produk \"{product.Name}\" berhasil dihapus.");
        if (_table is not null)
            await _table.ReloadServerData();
    }

    private async Task ExportToCsvAsync()
    {
        try
        {
            var request = new GetProductRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                IsActive = _filterIsActive,
                PageNumber = 1,
                PageSize = 5000,
            };
            var result = await _productService.GetProductsAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var rows = result.Result!.Items;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Nama Produk,Deskripsi,Kategori,Harga,Status");

            foreach (var p in rows)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(p.Name),
                    CsvEscape(p.Description ?? ""),
                    CsvEscape(p.CategoryName ?? ""),
                    p.Price.ToString("F2"),
                    p.IsActive ? "Aktif" : "Nonaktif"
                ));
            }

            var csvBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var base64 = Convert.ToBase64String(csvBytes);
            var fileName = $"Produk_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName, "text/csv", base64);
            _snackbar.AddSuccess($"Export CSV berhasil: {rows.Count()} produk.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal export CSV: {ex.Message}");
        }
    }

    private async Task ExportToExcelAsync()
    {
        try
        {
            var request = new GetProductRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                IsActive = _filterIsActive,
                PageNumber = 1,
                PageSize = 5000,
            };
            var result = await _productService.GetProductsAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var rows = result.Result!.Items;
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Produk");

            var headers = new[] { "Nama Produk", "Deskripsi", "Kategori", "Harga (Rp)", "Status" };
            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#6F4E37");
                ws.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            var row = 2;
            foreach (var p in rows)
            {
                ws.Cell(row, 1).Value = p.Name;
                ws.Cell(row, 2).Value = p.Description ?? "";
                ws.Cell(row, 3).Value = p.CategoryName ?? "";
                ws.Cell(row, 4).Value = (double)p.Price;
                ws.Cell(row, 5).Value = p.IsActive ? "Aktif" : "Nonaktif";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var base64 = Convert.ToBase64String(stream.ToArray());
            var fileName = $"Produk_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            await _jsRuntime.InvokeVoidAsync("downloadFileFromBase64", fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", base64);
            _snackbar.AddSuccess($"Export Excel berhasil: {rows.Count()} produk.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal export Excel: {ex.Message}");
        }
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
