using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Documents.Commands.CreateDocument;

public class CreateDocumentResponse : Response
{
    public Guid DocumentId { get; set; }
}
