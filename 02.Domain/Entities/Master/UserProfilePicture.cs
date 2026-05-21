using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Master;

/// <summary>Menyimpan informasi file foto profil pengguna.</summary>
public class UserProfilePicture : FileEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}
