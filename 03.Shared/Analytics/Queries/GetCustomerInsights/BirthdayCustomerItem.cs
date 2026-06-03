namespace Smlp.Shared.Analytics.Queries.GetCustomerInsights;

public class BirthdayCustomerItem
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string MemberCode { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDate { get; set; }
    public int TotalPoints { get; set; }

    /// <summary>Nama tier pelanggan</summary>
    public string Tier { get; set; } = string.Empty;

    /// <summary>Berapa hari lagi sampai ulang tahun (0 = hari ini)</summary>
    public int DaysUntilBirthday { get; set; }
}
