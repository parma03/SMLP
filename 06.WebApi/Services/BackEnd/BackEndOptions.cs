namespace Psmp.WebApi.Services.BackEnd;

public class BackEndOptions
{
    public const string SectionKey = nameof(BackEnd);

    public string BasePath { get; set; } = default!;
}
