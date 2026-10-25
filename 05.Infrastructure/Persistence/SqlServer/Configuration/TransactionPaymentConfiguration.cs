using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Transaction;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class TransactionPaymentConfiguration : IEntityTypeConfiguration<TransactionPayment>
{
    public void Configure(EntityTypeBuilder<TransactionPayment> builder)
    {
        builder.ToTable(TableNameFor.TransactionPayment, TableGroupFor.Transaction);
        builder.ConfigureCreatableProperties();

        builder.Property(e => e.PaymentMethod)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Reference)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));

        builder.HasIndex(e => e.TransactionId);

        builder.HasOne(e => e.Transaction)
            .WithMany(t => t.Payments)
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
