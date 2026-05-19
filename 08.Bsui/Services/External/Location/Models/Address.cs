using Newtonsoft.Json;

namespace Psmp.Bsui.Services.External.Location.Models;

public class Address
{
    [JsonProperty("city_block")]
    public string CityBlock { get; set; } = default!;

    [JsonProperty("neighbourhood")]
    public string Neighbourhood { get; set; } = default!;

    [JsonProperty("suburb")]
    public string Suburb { get; set; } = default!;

    [JsonProperty("city_district")]
    public string CityDistrict { get; set; } = default!;

    [JsonProperty("city")]
    public string City { get; set; } = default!;

    [JsonProperty("postcode")]
    public string PostalCode { get; set; } = default!;

    [JsonProperty("country")]
    public string Country { get; set; } = default!;

    [JsonProperty("country_code")]
    public string CountryCode { get; set; } = default!;
}
