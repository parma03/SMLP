/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 * Resilience / Retry Mechanism untuk RestSharp
 *
 * Karena project menggunakan RestSharp (bukan HttpClient), retry diimplementasikan
 * sebagai extension method ExecuteWithRetryAsync() yang membungkus RestClient.ExecuteAsync()
 * dengan logika exponential back-off sederhana tanpa dependency Polly tambahan.
 *
 * Jika di masa depan ingin migrasi ke HttpClient + Polly, lihat catatan di bawah.
 */

using System.Net;
using RestSharp;

namespace Smlp.Client.Common.Resilience;

/// <summary>
/// Konfigurasi retry untuk request RestSharp.
/// </summary>
public sealed class RetryOptions
{
    /// <summary>Jumlah maksimum percobaan ulang (tidak termasuk percobaan pertama).</summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>Delay awal sebelum retry pertama (milliseconds).</summary>
    public int InitialDelayMs { get; init; } = 500;

    /// <summary>
    /// Status HTTP yang akan memicu retry.
    /// Default: 408 (Timeout), 429 (Too Many Requests), 500, 502, 503, 504.
    /// </summary>
    public IReadOnlyCollection<HttpStatusCode> RetryableStatusCodes { get; init; } =
    [
        HttpStatusCode.RequestTimeout,       // 408
        HttpStatusCode.TooManyRequests,      // 429
        HttpStatusCode.InternalServerError,  // 500
        HttpStatusCode.BadGateway,           // 502
        HttpStatusCode.ServiceUnavailable,   // 503
        HttpStatusCode.GatewayTimeout,       // 504
    ];
}

/// <summary>
/// Extension methods untuk RestClient yang menambahkan retry otomatis dengan exponential back-off.
/// </summary>
public static class RestClientResilienceExtensions
{
    private static readonly RetryOptions DefaultOptions = new();

    /// <summary>
    /// Eksekusi RestRequest dengan retry otomatis menggunakan exponential back-off.
    /// Hanya method GET yang di-retry secara default untuk menghindari double-submit.
    /// </summary>
    /// <param name="client">RestClient</param>
    /// <param name="request">RestRequest yang akan dieksekusi</param>
    /// <param name="options">Konfigurasi retry (optional, gunakan default jika null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RestResponse dari percobaan terakhir</returns>
    public static async Task<RestResponse> ExecuteWithRetryAsync(
        this RestClient client,
        RestRequest request,
        RetryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= DefaultOptions;

        // Hanya GET yang di-retry secara aman (idempotent)
        // POST/PUT/DELETE tidak di-retry untuk menghindari side effect ganda
        var isIdempotent = request.Method is Method.Get or Method.Head or Method.Options;

        var attempt = 0;
        var maxAttempts = isIdempotent ? options.MaxRetryAttempts + 1 : 1;

        RestResponse response = null!;

        while (attempt < maxAttempts)
        {
            attempt++;

            try
            {
                response = await client.ExecuteAsync(request, cancellationToken);

                // Sukses atau status code yang tidak perlu di-retry
                if (response.IsSuccessful || !ShouldRetry(response, options))
                    return response;
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                // Jika di-cancel oleh user, jangan retry
                throw;
            }
            catch (Exception)
            {
                // Network error, connection refused, dll — retry
                if (attempt >= maxAttempts)
                    throw;
            }

            if (attempt < maxAttempts)
            {
                // Exponential back-off: delay = initialDelay * 2^(attempt-1)
                // Contoh dengan InitialDelayMs=500: 500ms, 1000ms, 2000ms
                var delayMs = options.InitialDelayMs * (int)Math.Pow(2, attempt - 1);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        return response;
    }

    private static bool ShouldRetry(RestResponse response, RetryOptions options)
    {
        if (response.StatusCode == 0)
            return true; // Tidak ada response (network error)

        return options.RetryableStatusCodes.Contains(response.StatusCode);
    }
}
