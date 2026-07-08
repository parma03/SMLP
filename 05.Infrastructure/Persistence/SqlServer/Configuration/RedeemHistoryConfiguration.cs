using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Transaction;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class RedeemHistoryConfiguration : IEntityTypeConfiguration<RedeemHistory>
{
    public void Configure(EntityTypeBuilder<RedeemHistory> builder)
    {
        builder.ToTable(TableNameFor.RedeemHistory, TableGroupFor.Transaction);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.RedeemCode)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
        builder.HasIndex(e => e.RedeemCode).IsUnique();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.VerifiedBy)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.CreatedBy));

        builder.Property(e => e.Notes)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.Property(e => e.RewardNameSnapshot)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        // Index untuk query optimization (list, filter, dan analytics)
        builder.HasIndex(e => e.RedeemDate);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => new { e.CustomerId, e.RedeemDate });
        builder.HasIndex(e => e.RewardId);
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.RedeemHistories)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Reward)
            .WithMany()
            .HasForeignKey(e => e.RewardId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
