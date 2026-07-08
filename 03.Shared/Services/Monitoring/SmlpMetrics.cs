using System.Diagnostics.Metrics;

namespace Smlp.Shared.Services.Monitoring;

/// <summary>
/// Instrumen metrik bisnis SMLP (Meter berbasis System.Diagnostics.Metrics, bawaan .NET).
/// Sengaja ditempatkan di 03.Shared — bukan di 05.Infrastructure — karena command handler
/// yang merekam metrik (mis. CreateTransactionCommand) berada di 04.Application, yang
/// tidak boleh mereferensikan 05.Infrastructure. Meter di sini hanya "mendefinisikan"
/// instrumen; OpenTelemetry SDK (dikonfigurasi di 05.Infrastructure/Monitoring) yang
/// meng-collect & meng-export nilainya ke Prometheus/Application Insights.
/// </summary>
public static class SmlpMetrics
{
    /// <summary>Nama Meter yang harus didaftarkan lewat `.AddMeter(SmlpMetrics.MeterName)` di sisi OpenTelemetry.</summary>
    public const string MeterName = "Smlp.Business";

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    /// <summary>Jumlah transaksi yang berhasil dibuat. Tag: payment_method.</summary>
    public static readonly Counter<long> TransactionsCreated = Meter.CreateCounter<long>(
        name: "smlp.transactions.created",
        unit: "{transaction}",
        description: "Jumlah transaksi yang berhasil dibuat");

    /// <summary>Distribusi nilai (Rupiah) per transaksi — berguna untuk histogram/percentile di Grafana.</summary>
    public static readonly Histogram<double> TransactionAmount = Meter.CreateHistogram<double>(
        name: "smlp.transactions.amount",
        unit: "IDR",
        description: "Distribusi grand total per transaksi dalam Rupiah");

    /// <summary>Total poin loyalty yang diterbitkan dari transaksi.</summary>
    public static readonly Counter<long> PointsEarned = Meter.CreateCounter<long>(
        name: "smlp.points.earned",
        unit: "{point}",
        description: "Total poin yang diterbitkan dari transaksi");

    /// <summary>Jumlah redeem yang diverifikasi. Tag: status (Completed/Rejected).</summary>
    public static readonly Counter<long> RedeemsVerified = Meter.CreateCounter<long>(
        name: "smlp.redemptions.verified",
        unit: "{redemption}",
        description: "Jumlah redeem poin yang diverifikasi (disetujui/ditolak)");
}
