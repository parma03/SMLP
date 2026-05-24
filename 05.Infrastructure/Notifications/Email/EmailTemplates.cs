namespace Smlp.Infrastructure.Notifications.Email;

public static class EmailTemplates
{
    private static string BaseLayout(string content, string title) => $"""
        <!DOCTYPE html>
        <html lang="id">
        <head>
          <meta charset="UTF-8"/>
          <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
          <title>{title}</title>
        </head>
        <body style="margin:0;padding:0;background:#f5f5f5;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f5f5f5;padding:30px 0;">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1);">
                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(135deg,#6F4E37,#4a3425);padding:30px 40px;text-align:center;">
                    <h1 style="margin:0;color:#ffffff;font-size:22px;letter-spacing:1px;">☕ SMLP Coffee Loyalty</h1>
                    <p style="margin:6px 0 0;color:rgba(255,255,255,0.8);font-size:13px;">Sistem Manajemen Loyalitas Pelanggan</p>
                  </td>
                </tr>
                <!-- Body -->
                <tr>
                  <td style="padding:36px 40px;">
                    {content}
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="background:#f9f4f0;padding:20px 40px;text-align:center;border-top:1px solid #e8ddd5;">
                    <p style="margin:0;font-size:12px;color:#888;">Email ini dikirim secara otomatis oleh SMLP. Jangan balas email ini.</p>
                    <p style="margin:6px 0 0;font-size:12px;color:#aaa;">© 2025-2026 SMLP Coffee Loyalty. All rights reserved.</p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    public static string TransactionConfirmation(string customerName, string transactionCode, decimal grandTotal, int pointsEarned)
    {
        var content = $"""
            <h2 style="margin:0 0 8px;color:#6F4E37;font-size:20px;">Terima kasih, {HtmlEncode(customerName)}! ☕</h2>
            <p style="margin:0 0 24px;color:#555;font-size:15px;">Transaksi Anda telah berhasil tercatat. Berikut rinciannya:</p>
            
            <table width="100%" cellpadding="0" cellspacing="0" style="background:#fdf8f4;border-radius:8px;padding:0;margin-bottom:24px;">
              <tr>
                <td style="padding:16px 20px;border-bottom:1px solid #e8ddd5;">
                  <span style="color:#888;font-size:13px;">Kode Transaksi</span><br/>
                  <strong style="color:#333;font-size:15px;">{HtmlEncode(transactionCode)}</strong>
                </td>
              </tr>
              <tr>
                <td style="padding:16px 20px;border-bottom:1px solid #e8ddd5;">
                  <span style="color:#888;font-size:13px;">Total Belanja</span><br/>
                  <strong style="color:#2E7D32;font-size:18px;">Rp {grandTotal:N0}</strong>
                </td>
              </tr>
              <tr>
                <td style="padding:16px 20px;">
                  <span style="color:#888;font-size:13px;">Poin Diperoleh</span><br/>
                  <strong style="color:#C8860A;font-size:18px;">⭐ +{pointsEarned} poin</strong>
                </td>
              </tr>
            </table>
            
            <p style="margin:0;color:#666;font-size:14px;">Kumpulkan poin dan tukarkan dengan reward menarik di program loyalitas kami. Sampai jumpa kembali! 👋</p>
            """;
        return BaseLayout(content, "Konfirmasi Transaksi - SMLP");
    }

    public static string TierUpgrade(string customerName, string oldTier, string newTier, int totalPoints)
    {
        var (emoji, color) = newTier.ToLower() switch
        {
            "gold" => ("🥇", "#C8860A"),
            "platinum" => ("💎", "#6366f1"),
            "silver" => ("🥈", "#607D8B"),
            _ => ("🥉", "#6F4E37"),
        };

        var content = $"""
            <div style="text-align:center;margin-bottom:28px;">
              <div style="font-size:56px;margin-bottom:8px;">{emoji}</div>
              <h2 style="margin:0;color:{color};font-size:22px;">Selamat, {HtmlEncode(customerName)}!</h2>
              <p style="margin:8px 0 0;color:#555;font-size:15px;">Tier Anda telah naik ke level berikutnya!</p>
            </div>
            
            <table width="100%" cellpadding="0" cellspacing="0" style="background:#fdf8f4;border-radius:8px;margin-bottom:24px;">
              <tr>
                <td width="50%" style="padding:20px;text-align:center;border-right:1px solid #e8ddd5;">
                  <p style="margin:0 0 4px;color:#888;font-size:12px;text-transform:uppercase;letter-spacing:1px;">Tier Sebelumnya</p>
                  <p style="margin:0;color:#999;font-size:18px;font-weight:600;">{HtmlEncode(oldTier)}</p>
                </td>
                <td width="50%" style="padding:20px;text-align:center;">
                  <p style="margin:0 0 4px;color:#888;font-size:12px;text-transform:uppercase;letter-spacing:1px;">Tier Baru</p>
                  <p style="margin:0;color:{color};font-size:20px;font-weight:700;">{emoji} {HtmlEncode(newTier)}</p>
                </td>
              </tr>
            </table>
            
            <p style="text-align:center;margin:0 0 16px;color:#555;font-size:14px;">Total lifetime poin Anda: <strong style="color:#C8860A;">⭐ {totalPoints:N0} poin</strong></p>
            <p style="text-align:center;margin:0;color:#666;font-size:13px;">Nikmati keuntungan eksklusif tier {HtmlEncode(newTier)} Anda. Terima kasih telah setia bersama kami! ☕</p>
            """;
        return BaseLayout(content, $"Naik Tier ke {newTier} - SMLP");
    }

    public static string RewardExpiryWarning(string customerName, int expiringPoints, DateOnly expiryDate)
    {
        var content = $"""
            <h2 style="margin:0 0 8px;color:#D32F2F;font-size:20px;">⚠️ Poin Anda Akan Segera Kadaluarsa!</h2>
            <p style="margin:0 0 24px;color:#555;font-size:15px;">Hai <strong>{HtmlEncode(customerName)}</strong>, jangan lewatkan poin loyalitas Anda!</p>
            
            <table width="100%" cellpadding="0" cellspacing="0" style="background:#fff8f8;border-radius:8px;border:1px solid #ffcdd2;margin-bottom:24px;">
              <tr>
                <td style="padding:20px;text-align:center;">
                  <p style="margin:0 0 4px;color:#888;font-size:13px;">Poin yang akan kadaluarsa</p>
                  <p style="margin:0;color:#D32F2F;font-size:28px;font-weight:700;">⭐ {expiringPoints:N0} poin</p>
                  <p style="margin:8px 0 0;color:#888;font-size:13px;">Kadaluarsa pada: <strong style="color:#D32F2F;">{expiryDate:dd MMMM yyyy}</strong></p>
                </td>
              </tr>
            </table>
            
            <p style="margin:0 0 16px;color:#555;font-size:14px;">Gunakan poin Anda sebelum kadaluarsa untuk mendapatkan reward menarik. Kunjungi toko kami atau hubungi kasir untuk menukarkan poin.</p>
            <p style="margin:0;color:#888;font-size:13px;">Butuh bantuan? Hubungi kami di kasir terdekat.</p>
            """;
        return BaseLayout(content, "Peringatan Poin Kadaluarsa - SMLP");
    }

    private static string HtmlEncode(string text) =>
        System.Net.WebUtility.HtmlEncode(text);
}
