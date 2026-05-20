using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Smlp.Bsui.Pages.Account;

/// <summary>
/// Halaman logout - JWT logout ditangani di Blazor component AccountInfo.
/// </summary>
public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // JWT logout sudah ditangani di Blazor (AuthorizedAuthenticationStateProvider.LogoutAsync)
        // Redirect ke Login
        return Redirect("/Login");
    }
}
