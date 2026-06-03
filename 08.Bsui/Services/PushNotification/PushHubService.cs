using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Smlp.Bsui.Common.Services.Notifications;
using Smlp.Bsui.Services.Authentication;

namespace Smlp.Bsui.Services.PushNotification;

/// <summary>
/// Background service yang menjaga koneksi SignalR ke NotificationHub WebApi.
/// Mendengarkan event "ReceivePushNotification" dan meneruskannya ke NotificationService (in-app).
/// Didaftarkan sebagai IHostedService sehingga berjalan sejak aplikasi start.
/// </summary>
public class PushHubService : IHostedService, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PushHubOptions _options;
    private readonly ILogger<PushHubService> _logger;

    private HubConnection? _hubConnection;
    private bool _started = false;

    public PushHubService(
        IServiceProvider serviceProvider,
        IOptions<PushHubOptions> options,
        ILogger<PushHubService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    // ─── IHostedService ────────────────────────────────────────────────────

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_started) return;
        _started = true;

        try
        {
            await ConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PushHubService: gagal konek saat startup — akan retry otomatis.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync(cancellationToken);
        }
    }

    // ─── Connection ─────────────────────────────────────────────────────────

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        var hubUrl = $"{_options.ApiBaseUrl.TrimEnd('/')}/hubs/notification";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Kirim JWT token agar hub bisa [Authorize]
                options.AccessTokenProvider = async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var authProvider = scope.ServiceProvider
                            .GetRequiredService<AuthorizedAuthenticationStateProvider>();
                        return await authProvider.GetAccessTokenAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PushHubService: gagal mendapatkan access token.");
                        return null;
                    }
                };
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(60)
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Warning);
            })
            .Build();

        // ─── Event listeners ────────────────────────────────────────────

        _hubConnection.On<PushNotificationPayload>("ReceivePushNotification", payload =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider
                    .GetRequiredService<NotificationService>();

                var level = ParseLevel(payload.Level);

                notificationService.Add(
                    title: payload.Title,
                    message: payload.Message,
                    level: level,
                    actionUrl: payload.ActionUrl);

                _logger.LogDebug("PushHubService: terima notifikasi [{Level}] '{Title}'", payload.Level, payload.Title);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PushHubService: error saat handle ReceivePushNotification.");
            }
        });

        _hubConnection.Reconnecting += error =>
        {
            _logger.LogInformation("PushHubService: reconnecting... {Error}", error?.Message);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            _logger.LogInformation("PushHubService: reconnected (connectionId={ConnectionId})", connectionId);
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            if (error is not null)
                _logger.LogWarning("PushHubService: connection closed dengan error: {Error}", error.Message);
            else
                _logger.LogInformation("PushHubService: connection ditutup.");
            return Task.CompletedTask;
        };

        // ─── Start connection ────────────────────────────────────────────

        try
        {
            await _hubConnection.StartAsync(cancellationToken);
            _logger.LogInformation("PushHubService: terhubung ke {HubUrl}", hubUrl);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PushHubService: gagal terhubung ke {HubUrl}", hubUrl);
            // Tidak throw — WithAutomaticReconnect akan coba ulang
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static NotificationLevel ParseLevel(string level) => level?.ToLowerInvariant() switch
    {
        "success" => NotificationLevel.Success,
        "warning" => NotificationLevel.Warning,
        "error"   => NotificationLevel.Error,
        _         => NotificationLevel.Info,
    };

    // ─── IAsyncDisposable ───────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
