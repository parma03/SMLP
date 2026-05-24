/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Expired Points Background Job Interface
*/

namespace Smlp.Application.BackgroundJobs;

/// <summary>
/// Background job untuk mengkadaluarsakan poin pelanggan yang sudah melewati masa berlaku.
/// </summary>
public interface IExpiredPointsJob
{
    /// <summary>
    /// Menjalankan proses expiry poin berdasarkan konfigurasi LoyaltyPointExpireDays.
    /// Dipanggil setiap hari via recurring job Hangfire.
    /// </summary>
    Task RunAsync();
}
