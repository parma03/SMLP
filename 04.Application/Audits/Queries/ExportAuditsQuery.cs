using ClosedXML.Excel;
using MediatR;
using Smlp.Application.Audits;
using Smlp.Shared.Audits.Queries.ExportAudits;

namespace Smlp.Application.Audits.Queries;

public class ExportAuditsQuery : IRequest<ExportAuditsResponse>
{
    public IList<Guid> AuditIds { get; set; } = new List<Guid>();
}

public class ExportAuditsQueryHandler : IRequestHandler<ExportAuditsQuery, ExportAuditsResponse>
{
    private readonly IAuditRepository _auditRepository;

    public ExportAuditsQueryHandler(IAuditRepository auditRepository) => _auditRepository = auditRepository;

    public async Task<ExportAuditsResponse> Handle(ExportAuditsQuery request, CancellationToken cancellationToken)
    {
        var audits = await _auditRepository.GetAuditsByIdsAsync(request.AuditIds, cancellationToken);

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

        ws.Columns(1, 8).AdjustToContents();
        ws.Column(9).Width = 40;
        ws.Column(10).Width = 40;
        ws.Column(11).Width = 20;
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
