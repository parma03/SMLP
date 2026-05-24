/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Extension methods untuk RestResponse dengan auto-redirect 401/403
*/

using System.Net;
using Microsoft.AspNetCore.Components;
using Smlp.Client.Common.Extensions;
using Smlp.Shared.Common.Responses;
using RestSharp;

namespace Smlp.Client.Common.Handlers;

/// <summary>
/// Extension methods yang menggabungkan ToResponseResult() dengan pengecekan 401/403.
/// Contoh penggunaan:
/// <code>
///   var result = (await _restClient.ExecuteAsync(request))
///       .ToResponseResultWithRedirect&lt;MyResponse&gt;(_navigationManager);
/// </code>
/// </summary>
public static class RestResponseRedirectExtensions
{
    /// <summary>
    /// Konversi RestResponse ke ResponseResult. Jika 401 → redirect ke login.
    /// Jika 403 → redirect ke halaman Forbidden.
    /// </summary>
    public static ResponseResult<T> ToResponseResultWithRedirect<T>(
        this RestResponse restResponse,
        NavigationManager navigationManager) where T : Response
    {
        var result = restResponse.ToResponseResult<T>();

        if (result.Error is null) return result;

        if (result.Error.Status == HttpStatusCode.Unauthorized)
        {
            var currentUri = navigationManager.Uri;
            navigationManager.NavigateTo(
                $"/Account/Login?returnUrl={Uri.EscapeDataString(currentUri)}",
                forceLoad: true);
        }
        else if (result.Error.Status == HttpStatusCode.Forbidden)
        {
            navigationManager.NavigateTo("/Common/Pages/Errors/Forbidden");
        }

        return result;
    }
}
