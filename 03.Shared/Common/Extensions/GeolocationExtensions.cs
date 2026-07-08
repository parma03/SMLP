using Smlp.Base.ValueObjects;

namespace Smlp.Shared.Common.Extensions;

public static class GeolocationExtensions
{
    public static string LocationUrl(this Geolocation geolocation)
    {
        return $"https://www.google.com/maps?q={geolocation.Latitude},{geolocation.Longitude}";
    }

    public static string LocationText(this Geolocation geolocation)
    {
        if (!string.IsNullOrWhiteSpace(geolocation.City) || !string.IsNullOrWhiteSpace(geolocation.Country))
        {
            return string.Join(", ", new[] { geolocation.City, geolocation.Country }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        return geolocation.ToString();
    }
}
