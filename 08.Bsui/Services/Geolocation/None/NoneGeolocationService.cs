using Darnton.Blazor.DeviceInterop.Geolocation;

namespace Psmp.Bsui.Services.Geolocation.None;

public class NoneGeolocationService : IGeolocationService
{
    public event EventHandler<GeolocationEventArgs> WatchPositionReceived = default!;

    protected virtual void OnWatchPositionReceived(GeolocationEventArgs e)
    {
        WatchPositionReceived?.Invoke(this, e);
    }

    public Task ClearWatch(long watchId)
    {
        return default!;
    }

    public Task<GeolocationResult> GetCurrentPosition(PositionOptions options)
    {
        return default!;
    }

    public Task<long?> WatchPosition(PositionOptions options)
    {
        return default!;
    }
}
