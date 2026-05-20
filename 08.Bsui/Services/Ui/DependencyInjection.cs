using Smlp.Bsui.Services.UI.MudBlazorUI;

namespace Smlp.Bsui.Services.UI;

public static class DependencyInjection
{
    public static IServiceCollection AddUIService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMudBlazorUIService();
        return services;
    }
}
