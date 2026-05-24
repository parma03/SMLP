using FluentValidation;
using Smlp.Base.Customer.Enums;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Master.Customers.Commands.Create;

public class CreateCustomerRequest
{
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? EmailAddress { get; set; }
    public DateOnly? BirthDate { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(v => v.FullName)
            .NotEmpty().WithMessage("Nama lengkap tidak boleh kosong.")
            .MaximumLength(CommonMaximumLengthFor.LongText)
            .WithMessage($"Nama lengkap maksimal {CommonMaximumLengthFor.LongText} karakter.");

        RuleFor(v => v.PhoneNumber)
            .NotEmpty().WithMessage("Nomor telepon tidak boleh kosong.")
            .MaximumLength(CommonMaximumLengthFor.SimpleName)
            .WithMessage($"Nomor telepon maksimal {CommonMaximumLengthFor.SimpleName} karakter.")
            .Matches(@"^[0-9+\-\s()]+$").WithMessage("Format nomor telepon tidak valid.");

        RuleFor(v => v.EmailAddress)
            .EmailAddress().WithMessage("Format email tidak valid.")
            .MaximumLength(CommonMaximumLengthFor.LongText)
            .WithMessage($"Email maksimal {CommonMaximumLengthFor.LongText} karakter.")
            .When(v => !string.IsNullOrWhiteSpace(v.EmailAddress));

        RuleFor(v => v.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Tanggal lahir harus sebelum hari ini.")
            .When(v => v.BirthDate.HasValue);

        RuleFor(v => v.Address)
            .MaximumLength(CommonMaximumLengthFor.LargeText)
            .WithMessage($"Alamat maksimal {CommonMaximumLengthFor.LargeText} karakter.")
            .When(v => !string.IsNullOrWhiteSpace(v.Address));

        RuleFor(v => v.Notes)
            .MaximumLength(CommonMaximumLengthFor.LargeText)
            .WithMessage($"Catatan maksimal {CommonMaximumLengthFor.LargeText} karakter.")
            .When(v => !string.IsNullOrWhiteSpace(v.Notes));
    }
}
