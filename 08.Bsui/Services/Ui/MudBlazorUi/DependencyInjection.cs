using MudBlazor;
using MudBlazor.Services;

namespace Smlp.Bsui.Services.UI.MudBlazorUI;

public static class DependencyInjection
{
    public static IServiceCollection AddMudBlazorUIService(this IServiceCollection services)
    {
        services.AddMudServices();
        services.AddMudBlazorDialog();

        services.AddMudBlazorSnackbar(config =>
        {
            config.PositionClass = Defaults.Classes.Position.TopRight;
            config.PreventDuplicates = false;
            config.NewestOnTop = false;
            config.ShowCloseIcon = true;
            config.VisibleStateDuration = 10000;
            config.HideTransitionDuration = 500;
            config.ShowTransitionDuration = 500;
            config.SnackbarVariant = Variant.Filled;
        });

        return services;
    }
}
