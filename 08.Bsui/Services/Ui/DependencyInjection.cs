using Psmp.Bsui.Services.Ui.SyncfusionBlazor;
using Psmp.Bsui.Services.UI.MudBlazorUI;

namespace Psmp.Bsui.Services.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddUIService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMudBlazorUIService();
        services.AddSyncfusionService(configuration);
        return services;
    }
}
