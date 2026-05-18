using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.StudyMonitorings;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;
public class StudyMonitoringCommentConfiguration : IEntityTypeConfiguration<StudyMonitoringComment>
{
    public void Configure(EntityTypeBuilder<StudyMonitoringComment> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.StudyMonitoring)}.{nameof(IPsmpDbContext.StudyMonitoringComments)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Comment).HasColumnType(CommonColumnTypes.Ntext);
            builder.Property(e => e.User).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
        }
    }
}
