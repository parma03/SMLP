namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

public class RfmSegmentItem
{
    /// <summary>Nama segmen, e.g. "Champions", "Loyal", "At Risk", "Lost"</summary>
    public string Segment { get; set; } = default!;

    /// <summary>Label display bahasa Indonesia</summary>
    public string Label { get; set; } = default!;

    /// <summary>Deskripsi singkat segmen</summary>
    public string Description { get; set; } = default!;

    /// <summary>Jumlah pelanggan di segmen ini</summary>
    public int Count { get; set; }

    /// <summary>Warna MudBlazor: "success", "info", "warning", "error", "primary"</summary>
    public string Color { get; set; } = "default";

    /// <summary>Emoji ikon</summary>
    public string Icon { get; set; } = "📊";

    /// <summary>Rekomendasi aksi CRM</summary>
    public string ActionRecommendation { get; set; } = default!;
}
