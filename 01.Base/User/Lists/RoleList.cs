using Smlp.Base.User.Enums;

namespace Smlp.Base.User.Lists
{
    public static class RoleList
    {
        public static IList<UserRole> All()
        {
            return new List<UserRole>
            {
                UserRole.Admin,
                UserRole.Owner,
                UserRole.Kasir,
                UserRole.Pelanggan
            };
        }

        /// <summary>Role yang bisa mengakses sistem (bukan pelanggan)</summary>
        public static IList<UserRole> Staff()
        {
            return new List<UserRole>
            {
                UserRole.Admin,
                UserRole.Owner,
                UserRole.Kasir
            };
        }

        [Obsolete("Use All() instead")]
        public static IList<UserRole> UserRoleLists() => All();
    }
}
