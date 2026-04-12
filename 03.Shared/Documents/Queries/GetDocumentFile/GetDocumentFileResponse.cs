using Smlp.Base.Selections.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Documents.Queries.GetDocumentFile;

public class GetDocumentFileResponse : FileResponse
{
    public Guid Id { get; set; } = default!;
    public DocumentType DocumentType { get; set; }
}
