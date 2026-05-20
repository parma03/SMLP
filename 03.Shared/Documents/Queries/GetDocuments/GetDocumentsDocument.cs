using Smlp.Base.Selections.Enums;

namespace Smlp.Shared.Documents.Queries.GetDocuments;

public class GetDocumentsDocument
{
    public Guid Id { get; set; }

    public string Title { get; set; } = default!;
    public string DocumentCategoryName { get; set; } = default!;
    public int JrdpYear { get; set; }
    public string TopicCode { get; set; } = default!;
    public string? EcmContentId { get; set; }

    public string FileName { get; set; } = default!;
    public string FileContentType { get; set; } = default!;
    public long FileSize { get; set; }

    public DateTimeOffset Created { get; set; }
    public DocumentType DocumentType { get; set; }
}
