using MudBlazor;

namespace Smlp.Bsui.Layouts.Constants;

public class ThemeFor
{
    public static readonly MudTheme MainLayout = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#6F4E37",        // Coffee brown
            PrimaryLighten = "#8B6347",
            PrimaryDarken = "#4A3425",
            Secondary = "#C8860A",      // Gold/Amber - loyalty/premium feel
            SecondaryLighten = "#E09C1A",
            SecondaryDarken = "#A06808",
            Tertiary = "#2E7D32",       // Green for positive metrics
            AppbarBackground = "#3E2723", // Dark espresso
            AppbarText = "#FFF8E1",
            Background = "#FFF8F0",     // Warm cream background
            DrawerBackground = "#3E2723",
            DrawerText = "#FFF8E1",
            DrawerIcon = "#FFCCBC",
            Success = "#388E3C",
            Warning = "#F57C00",
            Error = "#C62828",
            Info = "#0277BD",
            Surface = "#FFFFFF",
            TextPrimary = "#3E2723",
            TextSecondary = "#795548",
        }
    };
}
