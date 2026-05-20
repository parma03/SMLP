namespace Smlp.Shared.Documents.Options;

public class DocumentOptions
{
    public const string SectionKey = nameof(Documents);

    public long MaximumFileSizeInBytes { get; set; }
    public string FolderPath { get; set; } = default!;
}
