using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Projects;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ProjectCharterRealizationConfiguration : IEntityTypeConfiguration<ProjectCharterRealization>
{
    public void Configure(EntityTypeBuilder<ProjectCharterRealization> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.InnovationProject)}.{nameof(IPsmpDbContext.ProjectCharterRealizations)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.MentorId).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.TinyText));
            builder.Property(e => e.MentorName).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.MentorEmail).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.CreatedBy));
            builder.Property(e => e.MentorPosition).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.InnovationProject).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.ProjectTitle).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.Abstract).HasColumnType(CommonColumnTypes.Ntext);
            builder.Property(e => e.ImpactUnit).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.TinyText));
            builder.Property(e => e.RealizationUnit).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.TinyText));
            builder.Property(e => e.TargetUnit).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.TinyText));
        }
    }
}
