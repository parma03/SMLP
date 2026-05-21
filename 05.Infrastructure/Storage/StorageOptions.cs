namespace Smlp.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionKey = "Storage";
    public string Provider { get; set; } = "LocalFolder";
}
