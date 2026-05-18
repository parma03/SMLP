/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Scholarships;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ScholarshipDecreeConfiguration : IEntityTypeConfiguration<ScholarshipDecree>
{
    public void Configure(EntityTypeBuilder<ScholarshipDecree> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Scholarship)}.{nameof(IPsmpDbContext.ScholarshipDecrees)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();
    }
}
