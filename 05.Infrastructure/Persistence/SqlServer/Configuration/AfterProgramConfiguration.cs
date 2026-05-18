using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.StudyMonitorings;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class AfterProgramConfiguration : IEntityTypeConfiguration<AfterProgram>
{
    public void Configure(EntityTypeBuilder<AfterProgram> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.StudyMonitoring)}.{nameof(IPsmpDbContext.AfterPrograms)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            //builder.Property(e => e.PresentationLink).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));

            builder.HasOne(e => e.Admission)
                       .WithOne(e => e.AfterProgram)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
