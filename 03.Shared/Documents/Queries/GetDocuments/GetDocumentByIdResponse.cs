
using Smlp.Base.Selections.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Documents.Queries.GetDocuments;
public class GetDocumentByIdResponse : Response
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = default!;
    public string FileContentType { get; set; } = default!;
    public long FileSize { get; set; }
    public DocumentType DocumentType { get; set; }
}
