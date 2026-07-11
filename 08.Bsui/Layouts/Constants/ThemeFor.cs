using MudBlazor;

namespace Smlp.Bsui.Layouts.Constants;

public static class ThemeFor
{
    public static readonly MudTheme MainLayout = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#6F4E37",              // Coffee brown
            PrimaryLighten = "#9C7A5C",
            PrimaryDarken = "#4A3425",
            PrimaryContrastText = "#FFFFFF",

            Secondary = "#C8860A",            // Gold/Amber - loyalty/premium feel
            SecondaryLighten = "#E0A93F",
            SecondaryDarken = "#A06808",
            SecondaryContrastText = "#FFFFFF",

            Tertiary = "#2E7D32",              // Green for positive metrics
            TertiaryContrastText = "#FFFFFF",

            AppbarBackground = "#3E2723",
            AppbarText = "#FFF8E1",

            Background = "#F6F1EA",            // Warm cream background
            BackgroundGray = "#EFE7DC",
            Surface = "#FFFFFF",

            DrawerBackground = "#3E2723",
            DrawerText = "#F3E9DD",
            DrawerIcon = "#E8B98E",

            Success = "#2E7D32",
            SuccessContrastText = "#FFFFFF",
            Warning = "#ED8A19",
            WarningContrastText = "#FFFFFF",
            Error = "#C62828",
            ErrorContrastText = "#FFFFFF",
            Info = "#0277BD",
            InfoContrastText = "#FFFFFF",

            TextPrimary = "#2E2019",
            TextSecondary = "#6B5B4F",
            TextDisabled = "rgba(46,32,25,0.4)",

            ActionDefault = "#6B5B4F",
            ActionDisabled = "rgba(46,32,25,0.3)",
            ActionDisabledBackground = "rgba(46,32,25,0.08)",

            LinesDefault = "rgba(111,78,55,0.14)",
            LinesInputs = "rgba(111,78,55,0.28)",
            TableLines = "rgba(111,78,55,0.10)",
            TableStriped = "rgba(111,78,55,0.035)",
            TableHover = "rgba(111,78,55,0.06)",

            Divider = "rgba(111,78,55,0.14)",
            DividerLight = "rgba(111,78,55,0.08)",

            GrayLight = "#F1E9DE",
            GrayLighter = "#F8F4EE",

            OverlayDark = "rgba(46,32,25,0.55)",

            HoverOpacity = 0.07,
        },

        PaletteDark = new PaletteDark()
        {
            Primary = "#D4A574",
            PrimaryContrastText = "#241812",

            Secondary = "#E0A93F",
            SecondaryContrastText = "#241812",

            Tertiary = "#66BB6A",

            AppbarBackground = "#1D1611",
            AppbarText = "#F3E9DD",

            Background = "#151009",
            BackgroundGray = "#1C1610",
            Surface = "#211A14",

            DrawerBackground = "#1D1611",
            DrawerText = "#F3E9DD",
            DrawerIcon = "#D4A574",

            Success = "#66BB6A",
            Warning = "#FFA94D",
            Error = "#EF5350",
            Info = "#4FC3F7",

            TextPrimary = "#F3E9DD",
            TextSecondary = "#C7B7A6",

            LinesDefault = "rgba(255,255,255,0.10)",
            LinesInputs = "rgba(255,255,255,0.22)",
            TableLines = "rgba(255,255,255,0.08)",
            TableStriped = "rgba(255,255,255,0.03)",
            TableHover = "rgba(255,255,255,0.05)",

            Divider = "rgba(255,255,255,0.10)",
            DividerLight = "rgba(255,255,255,0.06)",

            OverlayDark = "rgba(0,0,0,0.6)",
        },

        Typography = new Typography()
        {
            Default = new DefaultTypography()
            {
                FontFamily = new[] { "Plus Jakarta Sans", "Segoe UI", "Helvetica", "Arial", "sans-serif" },
                FontSize = "0.875rem",
                LetterSpacing = ".01071em",
            },
            H1 = new H1Typography() { FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" }, FontWeight = "700" },
            H2 = new H2Typography() { FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" }, FontWeight = "700" },
            H3 = new H3Typography() { FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" }, FontWeight = "700" },
            H4 = new H4Typography() { FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" }, FontWeight = "700" },
            H5 = new H5Typography() { FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" }, FontWeight = "700" },
            H6 = new H6Typography() { FontFamily = new[] { "Plus Jakarta Sans", "sans-serif" }, FontWeight = "600" },
            Button = new ButtonTypography() { FontWeight = "600", TextTransform = "none", LetterSpacing = ".02em" },
            Subtitle1 = new Subtitle1Typography() { FontWeight = "600" },
            Body1 = new Body1Typography() { LineHeight = "1.6" },
            Body2 = new Body2Typography() { LineHeight = "1.55" },
        },

        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "280px",
            DrawerMiniWidthLeft = "72px",
            AppbarHeight = "64px",
        },

        Shadows = new Shadow(),

        ZIndex = new ZIndex(),
    };
}
