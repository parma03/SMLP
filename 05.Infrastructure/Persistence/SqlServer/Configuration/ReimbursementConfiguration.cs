using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Payments;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ReimbursementConfiguration : IEntityTypeConfiguration<Reimbursement>
{
    public void Configure(EntityTypeBuilder<Reimbursement> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Payment)}.{nameof(IPsmpDbContext.Reimbursements)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Notes).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));
            builder.Property(e => e.RequestId).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

            builder.HasOne(e => e.Applicant)
                       .WithMany(e => e.Reimbursements)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
