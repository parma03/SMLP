/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.StudyPreparations;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class EssayConfiguration : IEntityTypeConfiguration<Essay>
{
    public void Configure(EntityTypeBuilder<Essay> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.StudyPreparation)}.{nameof(IPsmpDbContext.Essays)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Applicant)
                       .WithMany(e => e.Essays)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
