using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Features.Customers;

public partial class Index
{
    private string _searchText = string.Empty;
    private string _filterTier = string.Empty;
    private string _filterSegment = string.Empty;
    private bool _loading = false;

    // TODO: Replace with actual service call
    private List<CustomerListItem> _customers = new();

    public class CustomerListItem
    {
        public Guid Id { get; set; }
        public string MemberCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Tier { get; set; } = "Bronze";
        public string Segment { get; set; } = "New";
        public int TotalPoints { get; set; }
        public DateTimeOffset? LastTransactionDate { get; set; }
    }

    private Color GetTierColor(string tier) => tier switch
    {
        "Gold" => Color.Warning,
        "Silver" => Color.Default,
        "Platinum" => Color.Primary,
        _ => Color.Default
    };

    private Color GetSegmentColor(string segment) => segment switch
    {
        "Loyal" => Color.Success,
        "Active" => Color.Info,
        "AtRisk" => Color.Warning,
        "Inactive" => Color.Error,
        _ => Color.Default
    };
}
