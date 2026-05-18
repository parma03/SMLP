using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Universities;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
{
    public void Configure(EntityTypeBuilder<Admission> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.University)}.{nameof(IPsmpDbContext.Admissions)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Applicant)
                       .WithMany(e => e.Admissions)
                       .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.UniversityRecommendation)
                       .WithMany(e => e.Admission)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
