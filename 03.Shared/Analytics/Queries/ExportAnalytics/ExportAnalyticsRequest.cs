using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Analytics.Queries.ExportAnalytics;

public class ExportAnalyticsRequest
{
    /// <summary>Jenis export: "revenue", "loyalty", "customer-insights", "dashboard"</summary>
    public string Type { get; set; } = "revenue";

    /// <summary>Tahun untuk laporan revenue</summary>
    public int Year { get; set; } = DateTime.Now.Year;

    /// <summary>Bulan (0 = semua bulan)</summary>
    public int Month { get; set; } = 0;
}

public class ExportAnalyticsResponse : FileResponse { }
