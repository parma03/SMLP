namespace Psmp.Bsui.Services.AppInfo;

public class AppInfoOptions
{
    public const string SectionKey = nameof(AppInfo);

    public string FullName { get; set; } = default!;
    public string Description { get; set; } = default!;
}
