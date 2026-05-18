using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Projects;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ProjectMentoringConfiguration : IEntityTypeConfiguration<ProjectMentoring>
{
    public void Configure(EntityTypeBuilder<ProjectMentoring> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.InnovationProject)}.{nameof(IPsmpDbContext.ProjectMentorings)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            //builder.Property(e => e.PresentationLink).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            //builder.Property(e => e.MateriLink).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.Title).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.MentorId).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.TinyText));
            builder.Property(e => e.MentorPosition).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
        }
    }
}
