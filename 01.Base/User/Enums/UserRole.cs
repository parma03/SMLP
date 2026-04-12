using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.User.Enums
{
    public enum UserRole
    {
        [Description("Admin")]
        [Display(Name = "Admin")]
        Admin = 0,

        [Description("Owner")]
        [Display(Name = "Owner")]
        Owner = 1,

        [Description("Pelanggan")]
        [Display(Name = "Pelanggan")]
        Pelanggan = 2,
    }
}
