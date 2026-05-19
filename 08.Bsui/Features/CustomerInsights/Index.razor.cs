using MudBlazor;

namespace Smlp.Bsui.Features.CustomerInsights;

public partial class Index
{
    private List<SegmentRow> _segments = new()
    {
        new("Loyal", "Beli sering, baru, dan banyak", Color.Success, "Pertahankan dengan reward eksklusif"),
        new("Potensial", "Beli sering & banyak tapi baru mulai", Color.Info, "Tingkatkan dengan program upgrade tier"),
        new("Baru Aktif", "Baru bergabung, belanja pertama", Color.Primary, "Sambut dengan onboarding rewards"),
        new("Butuh Perhatian", "Pernah aktif, kini mulai jarang", Color.Warning, "Kirim penawaran comeback"),
        new("Berisiko Pergi", "Tidak beli dalam 60+ hari", Color.Error, "Kampanye win-back agresif"),
        new("Hilang", "Tidak aktif > 6 bulan", Color.Default, "Evaluasi dan arsipkan"),
    };

    public record SegmentRow(string Name, string Description, Color Color, string Action);
}
