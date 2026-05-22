/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Role-specific Authorize Attribute Helpers
*/

namespace Smlp.Application.Common.Attributes;

/// <summary>
/// Hanya Admin yang dapat mengakses handler/command/query ini.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute()
    {
        Policy = "AdminOnly";
    }
}

/// <summary>
/// Hanya Owner yang dapat mengakses handler/command/query ini.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class OwnerOnlyAttribute : AuthorizeAttribute
{
    public OwnerOnlyAttribute()
    {
        Policy = "OwnerOnly";
    }
}

/// <summary>
/// Hanya Kasir yang dapat mengakses handler/command/query ini.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class KasirOnlyAttribute : AuthorizeAttribute
{
    public KasirOnlyAttribute()
    {
        Policy = "KasirOnly";
    }
}

/// <summary>
/// Admin dan Owner dapat mengakses handler/command/query ini.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AdminOrOwnerAttribute : AuthorizeAttribute
{
    public AdminOrOwnerAttribute()
    {
        Policy = "AdminOrOwner";
    }
}

/// <summary>
/// Admin, Owner, dan Kasir dapat mengakses handler/command/query ini.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class AllRolesAttribute : AuthorizeAttribute
{
    public AllRolesAttribute()
    {
        Policy = "AllRoles";
    }
}
