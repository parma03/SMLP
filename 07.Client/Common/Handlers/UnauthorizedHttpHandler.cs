/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Handler untuk mendeteksi 401/403 dari API dan redirect otomatis
*/

using System.Net;
using Microsoft.AspNetCore.Components;
using RestSharp;
using Smlp.Shared.Common.Responses;

namespace Smlp.Client.Common.Handlers;

/// <summary>
/// Helper untuk memeriksa status respons HTTP dan melakukan redirect
/// ke halaman error yang sesuai (login untuk 401, forbidden untuk 403).
/// Gunakan method ekstensi <see cref="RestResponseRedirectExtensions.WithRedirect{T}"/> setelah ToResponseResult().
/// </summary>
public class UnauthorizedHttpHandler
{
    private readonly NavigationManager _navigationManager;

    public UnauthorizedHttpHandler(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Periksa ResponseResult dan redirect jika 401 atau 403.
    /// Mengembalikan true jika redirect dilakukan (panggil return setelah ini).
    /// </summary>
    public bool HandleIfUnauthorized<T>(ResponseResult<T> result) where T : Response
    {
        if (result.Error is null) return false;

        if (result.Error.Status == HttpStatusCode.Unauthorized)
        {
            _navigationManager.NavigateTo("/Account/Login?returnUrl=" +
                Uri.EscapeDataString(_navigationManager.Uri), forceLoad: true);
            return true;
        }

        if (result.Error.Status == HttpStatusCode.Forbidden)
        {
            _navigationManager.NavigateTo("/Common/Pages/Errors/Forbidden");
            return true;
        }

        return false;
    }
}
