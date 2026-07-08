namespace Smlp.Application.Common.Caching;

/// <summary>
/// Konstanta cache key yang dipakai bersama antara query (penyimpan cache)
/// dan command handler (yang meng-invalidate cache setelah mutasi data),
/// supaya tidak ada magic string yang tercecer di banyak file.
/// </summary>
public static class CacheKeys
{
    public const string DashboardSummary = "dashboard:summary";
}
