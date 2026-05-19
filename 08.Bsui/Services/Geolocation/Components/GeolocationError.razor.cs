using Microsoft.AspNetCore.Components;

namespace Psmp.Bsui.Services.Geolocation.Components;

public partial class GeolocationError
{
    [Parameter]
    public string ErrorMessage { get; set; } = default!;
}
