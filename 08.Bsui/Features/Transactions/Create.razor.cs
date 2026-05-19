using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Features.Transactions;

public partial class Create
{
    [Inject] ISnackbar Snackbar { get; set; } = default!;
    [Inject] NavigationManager Navigation { get; set; } = default!;

    private string _memberSearch = string.Empty;
    private string _paymentMethod = "Tunai";
    private string _notes = string.Empty;
    private CustomerInfo? _selectedCustomer;
    private List<TransactionItemModel> _items = new();

    private decimal _subtotal => _items.Sum(x => x.Price * x.Quantity);
    private int _estimatedPoints => (int)(_subtotal / 10000);

    private void AddItem()
    {
        _items.Add(new TransactionItemModel());
    }

    private void RemoveItem(TransactionItemModel item)
    {
        _items.Remove(item);
    }

    private async Task SaveTransaction()
    {
        // TODO: Call transaction service
        Snackbar.Add("Transaksi berhasil disimpan! Poin telah ditambahkan.", Severity.Success);
        Navigation.NavigateTo("/Transactions");
    }

    public class CustomerInfo
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Tier { get; set; } = "Bronze";
        public int TotalPoints { get; set; }
    }

    public class TransactionItemModel
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
