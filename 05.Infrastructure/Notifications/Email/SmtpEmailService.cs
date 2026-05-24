using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Smlp.Application.Services.Email;
using Smlp.Application.Services.Email.Models;

namespace Smlp.Infrastructure.Notifications.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var smtp = _settings.GetActiveSettings();
        if (smtp is null || string.IsNullOrWhiteSpace(smtp.Username))
        {
            _logger.LogWarning("Email service not configured. Skipping send to {To} (Subject: {Subject})", to, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp.DisplayName, smtp.Username));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            var secureOptions = smtp.UseStartTls
                ? SecureSocketOptions.StartTls
                : smtp.UseSsl
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.Auto;

            await client.ConnectAsync(smtp.Host, smtp.Port, secureOptions, cancellationToken);
            await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent to {To} (Subject: {Subject})", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} (Subject: {Subject})", to, subject);
            // Do NOT rethrow — email failure should not break the main flow
        }
    }

    public async Task SendTransactionConfirmationAsync(
        string to, string customerName, string transactionCode,
        decimal grandTotal, int pointsEarned,
        CancellationToken cancellationToken = default)
    {
        var html = EmailTemplates.TransactionConfirmation(customerName, transactionCode, grandTotal, pointsEarned);
        await SendAsync(to, $"Konfirmasi Transaksi {transactionCode} - SMLP", html, cancellationToken);
    }

    public async Task SendTierUpgradeAsync(
        string to, string customerName, string oldTier, string newTier,
        int totalPoints, CancellationToken cancellationToken = default)
    {
        var html = EmailTemplates.TierUpgrade(customerName, oldTier, newTier, totalPoints);
        await SendAsync(to, $"Selamat! Tier Anda Naik ke {newTier} - SMLP", html, cancellationToken);
    }

    public async Task SendRewardExpiryWarningAsync(
        string to, string customerName, int expiringPoints,
        DateOnly expiryDate, CancellationToken cancellationToken = default)
    {
        var html = EmailTemplates.RewardExpiryWarning(customerName, expiringPoints, expiryDate);
        await SendAsync(to, "⚠️ Poin Anda Akan Kadaluarsa - SMLP", html, cancellationToken);
    }
}

/// <summary>No-op email service for when email is disabled.</summary>
public class NullEmailService : IEmailService
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendTransactionConfirmationAsync(string to, string customerName, string transactionCode, decimal grandTotal, int pointsEarned, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendTierUpgradeAsync(string to, string customerName, string oldTier, string newTier, int totalPoints, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendRewardExpiryWarningAsync(string to, string customerName, int expiringPoints, DateOnly expiryDate, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
