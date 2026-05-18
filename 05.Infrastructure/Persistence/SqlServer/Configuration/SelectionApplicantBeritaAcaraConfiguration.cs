using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Selections;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class SelectionApplicantBeritaAcaraConfiguration : IEntityTypeConfiguration<SelectionApplicantBeritaAcara>
{
    public void Configure(EntityTypeBuilder<SelectionApplicantBeritaAcara> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Selection)}.{nameof(IPsmpDbContext.SelectionApplicantBeritaAcaras)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Applicant)
                .WithOne(e => e.SelectionApplicantBeritaAcara)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
