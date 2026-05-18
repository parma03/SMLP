using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Universities;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class AdmissionAgreementConfiguration : IEntityTypeConfiguration<AdmissionAgreement>
{
    public void Configure(EntityTypeBuilder<AdmissionAgreement> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.University)}.{nameof(IPsmpDbContext.AdmissionAgreements)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.SuperiorLetter).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));
            builder.Property(e => e.NumberOfLetter).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

            builder.HasOne(e => e.Applicant)
                       .WithOne(e => e.AdmissionAgreement)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
