using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.StudyMonitorings;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class GraduateConfiguration : IEntityTypeConfiguration<Graduate>
{
    public void Configure(EntityTypeBuilder<Graduate> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.StudyMonitoring)}.{nameof(IPsmpDbContext.Graduates)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Admission)
                       .WithOne(e => e.Graduate)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
