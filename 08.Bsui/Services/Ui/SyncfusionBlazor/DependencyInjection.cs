using Syncfusion.Licensing;

namespace Smlp.Bsui.Services.Ui.SyncfusionBlazor;

public static class DependencyInjection
{
    public static IServiceCollection AddSyncfusionService(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection(SyncfusionBlazorOption.SectionKey).Get<SyncfusionBlazorOption>();
        SyncfusionLicenseProvider.RegisterLicense(option is not null ? option.LicenseKey : string.Empty);

        return services;
    }
}
