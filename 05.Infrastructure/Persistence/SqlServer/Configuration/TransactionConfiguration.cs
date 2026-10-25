using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Transaction;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaksi>
{
    public void Configure(EntityTypeBuilder<Transaksi> builder)
    {
        builder.ToTable(TableNameFor.Transaction, TableGroupFor.Transaction);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.TransactionCode)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
        builder.HasIndex(e => e.TransactionCode).IsUnique();

        builder.Property(e => e.PaymentMethod)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.Notes)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.GrandTotal)
            .HasPrecision(18, 2);

        builder.Property(e => e.PointMultiplierSnapshot)
            .HasPrecision(18, 2);

        builder.Property(e => e.CustomerTierSnapshot)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        // Index untuk query optimization (list, filter, dan analytics)
        // Catatan: semua index di bawah diberi filter IsDeleted = 0 karena
        // seluruh query transaksi selalu melalui global soft-delete query filter
        // (lihat ModelBuilderExtensions.SetSoftDeleteQueryFilter), sehingga index
        // yang hanya mencakup baris aktif jauh lebih kecil & efisien.

        // Dipakai untuk sort default (OrderByDescending TransactionDate) di
        // GetTransactionByListQuery & GetTransactionReportQuery.
        builder.HasIndex(e => e.TransactionDate)
            .HasFilter("[IsDeleted] = 0");

        // Composite (CustomerId, TransactionDate) mencakup query berbasis CustomerId
        // saja (leading column) sekaligus filter CustomerId + range tanggal,
        // sehingga index tunggal CustomerId tidak lagi diperlukan.
        builder.HasIndex(e => new { e.CustomerId, e.TransactionDate })
            .HasFilter("[IsDeleted] = 0");

        // Dipakai GetTransactionReportQuery & GetPointReportQuery yang selalu
        // filter Status == Completed lalu range tanggal + sort tanggal.
        builder.HasIndex(e => new { e.Status, e.TransactionDate })
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Transactions)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
