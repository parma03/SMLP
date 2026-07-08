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

        builder.Property(e => e.CustomerTierSnapshot)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        // Index untuk query optimization (list, filter, dan analytics)
        builder.HasIndex(e => e.TransactionDate);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => new { e.CustomerId, e.TransactionDate });
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Transactions)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
