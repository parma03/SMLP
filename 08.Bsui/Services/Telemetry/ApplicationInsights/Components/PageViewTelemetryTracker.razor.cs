using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Components.Routing;
using Psmp.Bsui.Common.Constants;

namespace Psmp.Bsui.Services.Telemetry.ApplicationInsights.Components;

public partial class PageViewTelemetryTracker
{
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _navigationManager.LocationChanged += OnLocationChanged;
        }
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var uri = new Uri(e.Location);
        var name = _navigationManager.ToBaseRelativePath(e.Location);

        if (e.Location == _navigationManager.BaseUri)
        {
            name = nameof(CommonRouteFor.Index);
        }
        else if (Guid.TryParse(uri.Segments.Last(), out var guid))
        {
            name = name.Replace($"/{guid}", string.Empty);
        }

        var pageViewTelemetry = new PageViewTelemetry
        {
            Url = uri,
            Name = name
        };

        if (_userInfo.Geolocation is not null)
        {
            pageViewTelemetry.Properties[nameof(Base.ValueObjects.Geolocation.Latitude)] = _userInfo.Geolocation.Latitude.ToString();
            pageViewTelemetry.Properties[nameof(Base.ValueObjects.Geolocation.Longitude)] = _userInfo.Geolocation.Longitude.ToString();
            pageViewTelemetry.Properties[nameof(Base.ValueObjects.Geolocation.Accuracy)] = _userInfo.Geolocation.Accuracy.ToString();
        }

        _telemetryClient.TrackPageView(pageViewTelemetry);
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
    }
}
