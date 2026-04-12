namespace Smlp.Shared.Common.Responses;

public class FileResponse : Response
{
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
