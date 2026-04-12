
using Microsoft.AspNetCore.Components.Forms;

namespace Smlp.Shared.Common.Responses;
public class FileUploadResponse : Response
{
    public string FileName { get; set; } = default!;
    public string FileContentType { get; set; } = default!;
    public double FileSize { get; set; }
    public string FilePath { get; set; } = default!;

    public IBrowserFile File { get; set; } = default!;

    public byte[] FileByteContent { get; set; } = Array.Empty<byte>();
}
