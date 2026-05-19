namespace Smlp.WebApi.Services.Documentation;

public class DocumentationOptions
{
    public const string SectionKey = nameof(Documentation);

    public string Provider { get; set; } = DocumentationProvider.None;
}

public static class DocumentationProvider
{
    public const string None = nameof(None);
    public const string Swagger = nameof(Swagger);
}
