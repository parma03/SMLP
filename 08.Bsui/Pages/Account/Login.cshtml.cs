using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Smlp.Bsui.Pages.Account;

/// <summary>
/// Halaman login hanya sebagai redirect ke Blazor /Login page.
/// Autentikasi JWT ditangani oleh Blazor component.
/// </summary>
public class LoginModel : PageModel
{
    public IActionResult OnGet(string? returnUrl)
    {
        // Redirect ke Blazor Login page
        return Redirect("/Login");
    }
}
