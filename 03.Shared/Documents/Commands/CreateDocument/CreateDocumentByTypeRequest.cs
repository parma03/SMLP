using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;
using Smlp.Base.Selections.Enums;

namespace Smlp.Shared.Documents.Commands.CreateDocument;

public class CreateDocumentByTypeRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public DocumentType DocumentType { get; set; } = default!;

    [OpenApiContentType(ContentTypes.TextPlain)]
    public string Filename { get; set; } = default!;
}
