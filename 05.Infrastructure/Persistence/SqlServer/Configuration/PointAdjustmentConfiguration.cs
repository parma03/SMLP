using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Transaction;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class PointAdjustmentConfiguration : IEntityTypeConfiguration<PointAdjustment>
{
    public void Configure(EntityTypeBuilder<PointAdjustment> builder)
    {
        builder.ToTable(TableNameFor.PointAdjustment, TableGroupFor.Transaction);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.AdjustmentType)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.Property(e => e.ApprovedBy)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.CreatedBy));

        // Index untuk query optimization (list, filter, dan analytics)
        // Composite (CustomerId, AdjustmentDate) sudah mencakup query CustomerId
        // saja (leading column), jadi index tunggal CustomerId dihapus.
        builder.HasIndex(e => e.AdjustmentDate)
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(e => new { e.CustomerId, e.AdjustmentDate })
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
