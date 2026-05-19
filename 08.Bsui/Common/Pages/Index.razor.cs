using MudBlazor;
using Smlp.Bsui.Common.Constants;

namespace Smlp.Bsui.Common.Pages;

public partial class Index
{
    private string _greetings = default!;

    protected override void OnInitialized()
    {
        var hour = DateTime.Now.Hour;
        _greetings = hour < 12 ? "Selamat Pagi" : hour < 15 ? "Selamat Siang" : hour < 18 ? "Selamat Sore" : "Selamat Malam";
    }
}
