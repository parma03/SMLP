using MudBlazor;

namespace Smlp.Bsui.Features.Transactions;

public partial class Index
{
    private string _searchText = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private bool _loading = false;
    private List<TransactionListItem> _transactions = new();

    public class TransactionListItem
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTimeOffset TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int PointsEarned { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Selesai";
    }
}
