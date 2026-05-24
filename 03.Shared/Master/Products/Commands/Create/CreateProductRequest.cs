using FluentValidation;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Master.Products.Commands.Create;

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public int Stock { get; set; } = 0;
    public Guid? CategoryId { get; set; }
}

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Nama produk tidak boleh kosong.")
            .MaximumLength(CommonMaximumLengthFor.LongText)
            .WithMessage($"Nama produk maksimal {CommonMaximumLengthFor.LongText} karakter.");

        RuleFor(v => v.Description)
            .MaximumLength(CommonMaximumLengthFor.LargeText)
            .WithMessage($"Deskripsi maksimal {CommonMaximumLengthFor.LargeText} karakter.")
            .When(v => !string.IsNullOrWhiteSpace(v.Description));

        RuleFor(v => v.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Harga tidak boleh negatif.");

        RuleFor(v => v.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok tidak boleh negatif.");
    }
}
