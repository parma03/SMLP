/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Selections;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class SelectionOverviewConfiguration : IEntityTypeConfiguration<SelectionOverview>
{
    public void Configure(EntityTypeBuilder<SelectionOverview> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Selection)}.{nameof(IPsmpDbContext.SelectionOverviews)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Applicant)
                .WithOne(e => e.SelectionOverviews)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
