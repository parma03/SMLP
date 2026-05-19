using Newtonsoft.Json;

namespace Smlp.Bsui.Services.External.Location.Models;

public class GeolocationDetails
{
    [JsonProperty("place_id")]
    public int PlaceId { get; set; }

    [JsonProperty("licence")]
    public string Licence { get; set; } = default!;

    [JsonProperty("osm_type")]
    public string OstType { get; set; } = default!;

    [JsonProperty("osm_id")]
    public int OsmId { get; set; }

    [JsonProperty("lat")]
    public string Latitude { get; set; } = default!;

    [JsonProperty("lon")]
    public string Longitude { get; set; } = default!;

    [JsonProperty("display_name")]
    public string DisplayName { get; set; } = default!;

    [JsonProperty("address")]
    public Address Address { get; set; } = default!;

    [JsonProperty("boundingbox")]
    public string[] BoundingBox { get; set; } = Array.Empty<string>();
}
