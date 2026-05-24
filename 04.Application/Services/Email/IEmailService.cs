/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Email Notification Service Interface
*/

namespace Smlp.Application.Services.Email;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendTransactionConfirmationAsync(string to, string customerName, string transactionCode, decimal grandTotal, int pointsEarned, CancellationToken cancellationToken = default);
    Task SendTierUpgradeAsync(string to, string customerName, string oldTier, string newTier, int totalPoints, CancellationToken cancellationToken = default);
    Task SendRewardExpiryWarningAsync(string to, string customerName, int expiringPoints, DateOnly expiryDate, CancellationToken cancellationToken = default);
}
