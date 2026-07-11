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

        // Relasi UserProfilePicture.User dikonfigurasi secara eksplisit sebagai relasi
        // TERPISAH dari User.ProfilePicture (lihat UserConfiguration). FK yang dipakai
        // di sini adalah UserProfilePicture.UserId (UserProfilePicture = dependent side),
        // merepresentasikan "user pemilik file foto ini" (bisa dipakai untuk histori
        // upload, terlepas dari mana foto yang sedang aktif di User.ProfilePictureId).
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
