namespace Smlp.Bsui.Services.AppInfo;

public class AppInfoOptions
{
    public const string SectionKey = nameof(AppInfo);

    public string FullName { get; set; } = "Sistem Manajemen Loyalitas Pelanggan";
    public string ShortName { get; set; } = "SMLP";
    public string Description { get; set; } = "Platform CRM & Loyalty Management untuk Coffee Shop";
    public string Version { get; set; } = "1.0.0";
}
