/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* WhatsApp Notification Service Interface (via Fonnte API)
*/

namespace Smlp.Application.Services.WhatsApp;

public interface IWhatsAppService
{
    Task SendAsync(string phone, string message, CancellationToken cancellationToken = default);
    Task SendTransactionConfirmationAsync(string phone, string customerName, string transactionCode, decimal grandTotal, int pointsEarned, CancellationToken cancellationToken = default);
    Task SendTierUpgradeAsync(string phone, string customerName, string newTier, int totalPoints, CancellationToken cancellationToken = default);
    Task SendRewardExpiryWarningAsync(string phone, string customerName, int expiringPoints, DateOnly expiryDate, CancellationToken cancellationToken = default);
}
