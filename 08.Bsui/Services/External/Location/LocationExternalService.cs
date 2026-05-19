using Microsoft.Extensions.Options;
using Psmp.Bsui.Services.External.Location.Models;
using RestSharp;

namespace Psmp.Bsui.Services.External.Location;

public class LocationExternalService
{
    private readonly string _endpointPattern;
    private readonly RestClient _restClient;

    public LocationExternalService(IOptions<LocationExternalOptions> locationServiceOptions)
    {
        _endpointPattern = locationServiceOptions.Value.EndpointPattern;
        _restClient = new RestClient(locationServiceOptions.Value.BaseUrl);
    }

    public async Task<GeolocationDetails> GetGeolocationDetails(Base.ValueObjects.Geolocation geolocation)
    {
        if (geolocation is null)
        {
            return new GeolocationDetails();
        }

        try
        {
            var lat = geolocation.Latitude.ToString().Replace(',', '.');
            var lon = geolocation.Longitude.ToString().Replace(',', '.');

            var endPoint = _endpointPattern
                .Replace("[lat]", lat)
                .Replace("[lon]", lon);

            var restRequest = new RestRequest(endPoint, Method.Get);
            var restResponse = await _restClient.ExecuteAsync<GeolocationDetails>(restRequest);

            if (!restResponse.IsSuccessful)
            {
                if (restResponse.ErrorException is null)
                {
                    throw new Exception($"Failed to retreive Geolocation details from {restRequest.Resource}");
                }

                throw restResponse.ErrorException;
            }

            if (restResponse.Data is null)
            {
                return new GeolocationDetails();
            }

            return restResponse.Data;
        }
        catch
        {
            return new GeolocationDetails();
        }
    }
}
