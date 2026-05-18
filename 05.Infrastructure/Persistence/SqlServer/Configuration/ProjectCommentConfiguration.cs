using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Projects;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ProjectCommentConfiguration : IEntityTypeConfiguration<ProjectComment>
{
    public void Configure(EntityTypeBuilder<ProjectComment> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.InnovationProject)}.{nameof(IPsmpDbContext.ProjectComments)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Comment).HasColumnType(CommonColumnTypes.Ntext);
            builder.Property(e => e.User).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
        }
    }
}
