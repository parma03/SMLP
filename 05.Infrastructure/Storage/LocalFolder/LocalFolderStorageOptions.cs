namespace Smlp.Infrastructure.Storage.LocalFolder;

public class LocalFolderStorageOptions
{
    public const string SectionKey = "LocalFolderStorage";
    public string FolderPath { get; set; } = "uploads";
}
