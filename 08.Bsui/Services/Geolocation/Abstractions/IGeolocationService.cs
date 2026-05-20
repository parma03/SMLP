namespace Smlp.Bsui.Services.Geolocation.Abstractions;

public class GeolocationCoordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Accuracy { get; set; }
}

public class GeolocationPosition
{
    public GeolocationCoordinates Coords { get; set; } = new();
}

public class GeolocationResult
{
    public bool IsSuccess { get; set; }
    public GeolocationPosition? Position { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PositionOptions
{
    public bool EnableHighAccuracy { get; set; }
    public long? Timeout { get; set; }
    public long? MaximumAge { get; set; }
}

public class GeolocationEventArgs : EventArgs
{
    public GeolocationResult Result { get; set; } = new();
}

public interface IGeolocationService
{
    event EventHandler<GeolocationEventArgs> WatchPositionReceived;
    Task<GeolocationResult> GetCurrentPosition(PositionOptions? options = null);
    Task<long?> WatchPosition(PositionOptions? options = null);
    Task ClearWatch(long watchId);
}
