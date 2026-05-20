using Microsoft.JSInterop;
using Smlp.Bsui.Services.Geolocation.Abstractions;

namespace Smlp.Bsui.Services.Geolocation.Darnton;

public class BrowserGeolocationService : IGeolocationService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;

    public event EventHandler<GeolocationEventArgs> WatchPositionReceived = default!;

    public BrowserGeolocationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<GeolocationResult> GetCurrentPosition(PositionOptions? options = null)
    {
        try
        {
            var coords = await _jsRuntime.InvokeAsync<double[]>("smlpGeolocation.getCurrentPosition");
            return new GeolocationResult
            {
                IsSuccess = true,
                Position = new GeolocationPosition
                {
                    Coords = new GeolocationCoordinates
                    {
                        Latitude = coords[0],
                        Longitude = coords[1]
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new GeolocationResult { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    public Task<long?> WatchPosition(PositionOptions? options = null)
    {
        return Task.FromResult<long?>(null);
    }

    public Task ClearWatch(long watchId)
    {
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
