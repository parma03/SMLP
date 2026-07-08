using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Transaction;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
{
    public void Configure(EntityTypeBuilder<TransactionItem> builder)
    {
        builder.ToTable(TableNameFor.TransactionItem, TableGroupFor.Transaction);
        builder.ConfigureCreatableProperties();

        builder.Property(e => e.ProductName)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        builder.HasIndex(e => e.TransactionId);
        builder.HasIndex(e => e.ProductId);

        builder.HasOne(e => e.Transaction)
            .WithMany(t => t.Items)
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
