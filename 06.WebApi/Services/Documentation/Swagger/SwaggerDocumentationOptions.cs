namespace Psmp.WebApi.Services.Documentation.Swagger;

public class SwaggerDocumentationOptions
{
    public static readonly string SectionKey = $"{nameof(Documentation)}:{nameof(Swagger)}";

    public string SwaggerPrefix { get; set; } = default!;
    public string JsonEndpoint { get; set; } = default!;

    public string Description { get; set; } = default!;
    public string DescriptionMarkdownFile { get; set; } = default!;
    public string TermsOfServiceUrl { get; set; } = default!;

    public IList<string> ApiVersions { get; set; } = new List<string>();

    public Contact Contact { get; set; } = default!;
    public License License { get; set; } = default!;
    public Logo Logo { get; set; } = default!;
}

public class Contact
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Url { get; set; } = default!;
}

public class License
{
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;
}

public class Logo
{
    public string Url { get; set; } = default!;
    public string Text { get; set; } = default!;
}
