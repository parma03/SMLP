using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Universities;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class AdmissionSuperiorLetterConfiguration : IEntityTypeConfiguration<AdmissionSuperiorLetter>
{
    public void Configure(EntityTypeBuilder<AdmissionSuperiorLetter> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.University)}.{nameof(IPsmpDbContext.AdmissionSuperiorLetters)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Applicant)
                       .WithOne(e => e.AdmissionSuperiorLetter)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
