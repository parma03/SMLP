
using Smlp.Base.Selections.Enums;
using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Documents.Queries.GetDocumentFile;
public class GetDocumentFileRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public Guid DocumentId { get; set; }

    [OpenApiContentType(ContentTypes.TextPlain)]
    public DocumentType DocumentType { get; set; }
}
