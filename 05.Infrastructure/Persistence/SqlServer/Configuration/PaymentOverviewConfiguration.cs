using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Payments;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;
public class PaymentOverviewConfiguration : IEntityTypeConfiguration<PaymentOverview>
{
    public void Configure(EntityTypeBuilder<PaymentOverview> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Payment)}.{nameof(IPsmpDbContext.PaymentOverviews)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.RequestId).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.CostCenter).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.AssigmentNumber).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.Notes).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));
        }
    }
}
