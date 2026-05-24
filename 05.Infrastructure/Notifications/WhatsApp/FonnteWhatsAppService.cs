using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smlp.Application.Services.WhatsApp;
using Smlp.Application.Services.WhatsApp.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Smlp.Infrastructure.Notifications.WhatsApp;

public class FonnteWhatsAppService : IWhatsAppService
{
    private readonly WhatsAppSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FonnteWhatsAppService> _logger;

    private const string FonnteBaseUrl = "https://api.fonnte.com";

    public FonnteWhatsAppService(
        IOptions<WhatsAppSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<FonnteWhatsAppService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(string phone, string message, CancellationToken cancellationToken = default)
    {
        if (_settings.Provider != "Fonnte" || _settings.Fonnte is null || string.IsNullOrWhiteSpace(_settings.Fonnte.ApiToken))
        {
            _logger.LogWarning("WhatsApp service not configured. Skipping send to {Phone}", phone);
            return;
        }

        try
        {
            // Normalize phone: strip +, leading 0 -> 62 (Indonesia)
            var normalizedPhone = NormalizePhone(phone);

            var client = _httpClientFactory.CreateClient("Fonnte");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", _settings.Fonnte.ApiToken);

            var payload = new Dictionary<string, string>
            {
                { "target", normalizedPhone },
                { "message", message },
                { "countryCode", "62" }, // Indonesia default
            };

            if (!string.IsNullOrWhiteSpace(_settings.Fonnte.DeviceId))
                payload["device"] = _settings.Fonnte.DeviceId;

            var response = await client.PostAsJsonAsync($"{FonnteBaseUrl}/send", payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp sent to {Phone} via Fonnte", normalizedPhone);
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Fonnte returned {StatusCode}: {Body}", response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp to {Phone}", phone);
            // Do NOT rethrow — WA failure should not break main flow
        }
    }

    public async Task SendTransactionConfirmationAsync(
        string phone, string customerName, string transactionCode,
        decimal grandTotal, int pointsEarned,
        CancellationToken cancellationToken = default)
    {
        var msg = $"Halo {customerName}! ☕\n" +
                  $"Transaksi *{transactionCode}* berhasil.\n" +
                  $"Total: *Rp {grandTotal:N0}*\n" +
                  $"Poin diperoleh: *+{pointsEarned} poin* ⭐\n\n" +
                  $"Terima kasih telah berbelanja di SMLP Coffee! 🙏";
        await SendAsync(phone, msg, cancellationToken);
    }

    public async Task SendTierUpgradeAsync(
        string phone, string customerName, string newTier,
        int totalPoints, CancellationToken cancellationToken = default)
    {
        var emoji = newTier.ToLower() switch { "gold" => "🥇", "platinum" => "💎", "silver" => "🥈", _ => "🥉" };
        var msg = $"Selamat {customerName}! {emoji}\n" +
                  $"Tier Anda naik ke *{newTier}*!\n" +
                  $"Total poin: *{totalPoints:N0} poin* ⭐\n\n" +
                  $"Nikmati keuntungan eksklusif tier {newTier} Anda. Terima kasih! ☕";
        await SendAsync(phone, msg, cancellationToken);
    }

    public async Task SendRewardExpiryWarningAsync(
        string phone, string customerName, int expiringPoints,
        DateOnly expiryDate, CancellationToken cancellationToken = default)
    {
        var msg = $"⚠️ Halo {customerName}!\n" +
                  $"*{expiringPoints:N0} poin* Anda akan kadaluarsa pada *{expiryDate:dd/MM/yyyy}*.\n\n" +
                  $"Segera tukarkan poin Anda dengan reward menarik sebelum kadaluarsa! 🎁";
        await SendAsync(phone, msg, cancellationToken);
    }

    private static string NormalizePhone(string phone)
    {
        phone = phone.Replace("+", "").Replace("-", "").Replace(" ", "").Trim();
        if (phone.StartsWith("0"))
            phone = "62" + phone[1..];
        return phone;
    }
}

/// <summary>No-op WhatsApp service for when WA is disabled.</summary>
public class NullWhatsAppService : IWhatsAppService
{
    public Task SendAsync(string phone, string message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendTransactionConfirmationAsync(string phone, string customerName, string transactionCode, decimal grandTotal, int pointsEarned, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendTierUpgradeAsync(string phone, string customerName, string newTier, int totalPoints, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendRewardExpiryWarningAsync(string phone, string customerName, int expiringPoints, DateOnly expiryDate, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
