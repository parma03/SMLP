using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Master;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNameFor.User, TableGroupFor.Master);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.Username)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
        builder.HasIndex(e => e.Username).IsUnique();

        builder.Property(e => e.DisplayName)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        builder.Property(e => e.Role)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.PhoneNumber)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));

        builder.Property(e => e.EmailAddress)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
        builder.HasIndex(e => e.EmailAddress).IsUnique();

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.Property(e => e.RefreshToken)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.HasIndex(e => e.IsActive);

        // Relasi User.ProfilePicture dikonfigurasi secara eksplisit sebagai relasi
        // TERPISAH dari UserProfilePicture.User (lihat UserProfilePictureConfiguration).
        // FK yang dipakai di sini adalah User.ProfilePictureId (User = dependent side),
        // merepresentasikan "foto profil yang sedang aktif" untuk user ini.
        // WithMany() tanpa parameter berarti tidak ada navigation collection balik
        // dari sisi UserProfilePicture untuk relasi ini secara spesifik.
        builder.HasOne(e => e.ProfilePicture)
            .WithMany()
            .HasForeignKey(e => e.ProfilePictureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
