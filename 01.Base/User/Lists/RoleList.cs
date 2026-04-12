using Smlp.Base.User.Enums;

namespace Smlp.Base.User.Lists
{
    public static class RoleList
    {
        public static IList<UserRole> UserRoleLists()
        {
            var list = new List<UserRole>
            {
                UserRole.Admin,
                UserRole.Owner,
                UserRole.Pelanggan
            };

            return list;
        }
    }
}
