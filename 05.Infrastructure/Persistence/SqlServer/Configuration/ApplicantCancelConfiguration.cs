using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Applicants;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ApplicantCancelConfiguration : IEntityTypeConfiguration<ApplicantCancel>
{
    public void Configure(EntityTypeBuilder<ApplicantCancel> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Applicant)}.{nameof(IPsmpDbContext.ApplicantCancels)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Reason).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));
        }
    }
}
