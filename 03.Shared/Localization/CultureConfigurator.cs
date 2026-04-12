using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Smlp.Shared.Localization;

public sealed record LocalizationSettings
{
    public string BaseCulture { get; set; } = "en-US";
    public string? CurrencySymbol { get; set; }
    public int? CurrencyDecimalDigits { get; set; }
    public string? NumberGroupSeparator { get; set; }
    public string? NumberDecimalSeparator { get; set; }
}

public class CultureConfigurator
{
    public static void Configure(
        LocalizationSettings settings,
        ILogger logger
    )
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo(settings.BaseCulture).Clone();

        if (settings.CurrencySymbol is not null)
        {
            culture.NumberFormat.CurrencySymbol = settings.CurrencySymbol;
        }

        if (settings.CurrencyDecimalDigits.HasValue)
        {
            culture.NumberFormat.CurrencyDecimalDigits = settings.CurrencyDecimalDigits.Value;
        }

        if (settings.NumberGroupSeparator is not null)
        {
            culture.NumberFormat.NumberGroupSeparator = settings.NumberGroupSeparator;
        }

        if (settings.NumberDecimalSeparator is not null)
        {
            culture.NumberFormat.NumberDecimalSeparator = settings.NumberDecimalSeparator;
        }

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        logger.LogInformation(
            "Configured Culture: {Culture}, Currency={Currency}, DecimalDigits={Digits}",
            culture.Name,
            culture.NumberFormat.CurrencySymbol,
            culture.NumberFormat.CurrencyDecimalDigits
        );
    }
}
