using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Audits.Queries.ExportAudits;

namespace Smlp.Application.Audits.Queries;

public class ExportAuditsQuery : IRequest<ExportAuditsResponse>
{
    public IList<Guid> AuditIds { get; set; } = new List<Guid>();
}

public class ExportAuditsQueryHandler : IRequestHandler<ExportAuditsQuery, ExportAuditsResponse>
{
    private readonly ISmlpDbContext _context;

    public ExportAuditsQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<ExportAuditsResponse> Handle(ExportAuditsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Audits.AsQueryable();

        if (request.AuditIds.Any())
            query = query.Where(a => request.AuditIds.Contains(a.Id));

        var audits = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(10_000) // safety cap
            .Select(a => new
            {
                a.CreatedAt,
                a.CreatedBy,
                a.EntityName,
                a.ActionType,
                a.ActionName,
                a.EntityId,
                a.TableName,
                a.FromIpAddress,
                a.OldValues,
                a.NewValues,
                a.ClientApplicationId,
            })
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Audit Log");

        // Header row
        var headers = new[]
        {
            "Waktu", "Oleh", "Entitas", "Tipe Aksi", "Nama Aksi",
            "Entity ID", "Tabel", "IP Address", "Nilai Lama", "Nilai Baru", "App ID"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data rows
        int row = 2;
        foreach (var a in audits)
        {
            ws.Cell(row, 1).Value = a.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 2).Value = a.CreatedBy;
            ws.Cell(row, 3).Value = a.EntityName;
            ws.Cell(row, 4).Value = a.ActionType;
            ws.Cell(row, 5).Value = a.ActionName;
            ws.Cell(row, 6).Value = a.EntityId.ToString();
            ws.Cell(row, 7).Value = a.TableName;
            ws.Cell(row, 8).Value = a.FromIpAddress;
            ws.Cell(row, 9).Value = a.OldValues ?? "-";
            ws.Cell(row, 10).Value = a.NewValues;
            ws.Cell(row, 11).Value = a.ClientApplicationId;
            row++;
        }

        // Auto-fit columns (cap wide columns)
        ws.Columns(1, 8).AdjustToContents();
        ws.Column(9).Width = 40;
        ws.Column(10).Width = 40;
        ws.Column(11).Width = 20;

        // Freeze header
        ws.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ExportAuditsResponse
        {
            FileName = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Content = stream.ToArray(),
        };
    }
}
