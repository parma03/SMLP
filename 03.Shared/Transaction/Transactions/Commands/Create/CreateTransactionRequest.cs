using FluentValidation;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Transaction.Transactions.Commands.Create;

public class CreateTransactionRequest
{
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Metode pembayaran utama (dipakai jika Payments kosong / backward-compat).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Daftar pembayaran (untuk split/multiple payment).
    /// Jika diisi, PaymentMethod di atas diabaikan.
    /// </summary>
    public List<PaymentEntryRequest> Payments { get; set; } = new();

    public string? Notes { get; set; }

    /// <summary>Diskon nominal yang diterapkan ke total transaksi.</summary>
    public decimal DiscountAmount { get; set; } = 0;

    public List<CreateTransactionItemRequest> Items { get; set; } = new();
}

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(v => v.CustomerId)
            .NotEmpty().WithMessage("Customer harus dipilih.");

        RuleFor(v => v.Items)
            .NotEmpty().WithMessage("Transaksi harus memiliki minimal 1 item.")
            .Must(items => items.All(i => i.Quantity > 0))
            .WithMessage("Setiap item harus memiliki jumlah (quantity) minimal 1.")
            .Must(items => items.All(i => i.UnitPrice >= 0))
            .WithMessage("Harga satuan tidak boleh negatif.");

        RuleFor(v => v.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Diskon tidak boleh negatif.");

        RuleFor(v => v.Notes)
            .MaximumLength(CommonMaximumLengthFor.LargeText)
            .WithMessage($"Catatan maksimal {CommonMaximumLengthFor.LargeText} karakter.")
            .When(v => !string.IsNullOrWhiteSpace(v.Notes));

        // Validasi split payment: total pembayaran >= grandTotal
        When(v => v.Payments.Any(), () =>
        {
            RuleFor(v => v.Payments)
                .Must(payments => payments.All(p => p.Amount > 0))
                .WithMessage("Setiap pembayaran harus bernilai lebih dari 0.");
        });
    }
}

public class CreateTransactionItemRequestValidator : AbstractValidator<CreateTransactionItemRequest>
{
    public CreateTransactionItemRequestValidator()
    {
        RuleFor(v => v.ProductName)
            .NotEmpty().WithMessage("Nama produk tidak boleh kosong.")
            .MaximumLength(CommonMaximumLengthFor.LongText)
            .WithMessage($"Nama produk maksimal {CommonMaximumLengthFor.LongText} karakter.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Jumlah produk harus lebih dari 0.");

        RuleFor(v => v.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Harga satuan tidak boleh negatif.");
    }
}
