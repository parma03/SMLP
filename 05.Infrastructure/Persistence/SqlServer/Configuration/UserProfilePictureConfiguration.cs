using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Master;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class UserProfilePictureConfiguration : IEntityTypeConfiguration<UserProfilePicture>
{
    public void Configure(EntityTypeBuilder<UserProfilePicture> builder)
    {
        builder.ToTable(TableNameFor.UserProfilePicture, TableGroupFor.Master);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();
        builder.ConfigureFileProperties();

        builder.HasIndex(e => e.UserId);
    }
}
