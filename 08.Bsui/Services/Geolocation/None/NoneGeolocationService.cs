using Smlp.Bsui.Services.Geolocation.Abstractions;

namespace Smlp.Bsui.Services.Geolocation.None;

public class NoneGeolocationService : IGeolocationService
{
    public event EventHandler<GeolocationEventArgs> WatchPositionReceived = default!;

    protected virtual void OnWatchPositionReceived(GeolocationEventArgs e)
    {
        WatchPositionReceived?.Invoke(this, e);
    }

    public Task ClearWatch(long watchId)
    {
        return Task.CompletedTask;
    }

    public Task<GeolocationResult> GetCurrentPosition(PositionOptions? options = null)
    {
        return Task.FromResult(new GeolocationResult { IsSuccess = false, ErrorMessage = "Geolocation not enabled." });
    }

    public Task<long?> WatchPosition(PositionOptions? options = null)
    {
        return Task.FromResult<long?>(null);
    }
}
