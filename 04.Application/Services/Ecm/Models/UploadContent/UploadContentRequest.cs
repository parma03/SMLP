using Smlp.Base.Document.Enums;

namespace Smlp.Application.Services.Ecm.Models.UploadContent;

public class UploadContentRequest
{
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = default!;
    public string FileContentType { get; set; } = default!;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
}
