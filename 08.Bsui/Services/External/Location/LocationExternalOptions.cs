namespace Smlp.Bsui.Services.External.Location;

public class LocationExternalOptions
{
    public static readonly string SectionKey = $"{nameof(External)}:{nameof(Location)}";

    public string BaseUrl { get; set; } = default!;
    public string EndpointPattern { get; set; } = default!;
}
