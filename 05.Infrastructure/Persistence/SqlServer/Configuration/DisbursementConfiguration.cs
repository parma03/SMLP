using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Payments;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class DisbursementConfiguration : IEntityTypeConfiguration<Disbursement>
{
    public void Configure(EntityTypeBuilder<Disbursement> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Payment)}.{nameof(IPsmpDbContext.Disbursements)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Notes).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));
            builder.Property(e => e.RequestId).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.City).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.Country).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

            builder.HasOne(e => e.Applicant)
                       .WithMany(e => e.Disbursements)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
