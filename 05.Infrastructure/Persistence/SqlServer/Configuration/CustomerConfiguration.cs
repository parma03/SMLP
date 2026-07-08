using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Master;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(TableNameFor.Customer, TableGroupFor.Master);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.MemberCode)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
        builder.HasIndex(e => e.MemberCode).IsUnique();

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
        builder.HasIndex(e => e.PhoneNumber);

        builder.Property(e => e.EmailAddress)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        builder.Property(e => e.Address)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.Property(e => e.Tier)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.Notes)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        // Index untuk query optimization (dashboard, insights, filter)
        // Filter IsDeleted = 0 karena Customer tunduk pada global soft-delete
        // query filter (ModelBuilderExtensions.SetSoftDeleteQueryFilter),
        // sehingga index hanya mencakup baris aktif.
        builder.HasIndex(e => e.Tier)
            .HasFilter("[IsDeleted] = 0");

        // Dipakai GetCustomerInsightsQuery untuk churn risk (LastTransactionDate < cutoff).
        builder.HasIndex(e => e.LastTransactionDate)
            .HasFilter("[IsDeleted] = 0");

        // Dipakai untuk hitung NewCount (CreatedAt >= cutoffNew).
        builder.HasIndex(e => e.CreatedAt)
            .HasFilter("[IsDeleted] = 0");

        // Dipakai GetCustomerInsightsQuery untuk top spenders
        // (Where TotalSpend > 0, OrderByDescending TotalSpend, Take 10).
        builder.HasIndex(e => e.TotalSpend)
            .HasFilter("[IsDeleted] = 0 AND [TotalSpend] > 0");

        // Dipakai untuk join/lookup Customer <-> User (login).
        builder.HasIndex(e => e.UserId);
    }
}
