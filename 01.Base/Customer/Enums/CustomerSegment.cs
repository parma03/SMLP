using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.Customer.Enums
{
    public enum CustomerSegment
    {
        [Description("Baru")]
        [Display(Name = "Baru")]
        New = 0,

        [Description("Aktif")]
        [Display(Name = "Aktif")]
        Active = 1,

        [Description("Setia")]
        [Display(Name = "Setia")]
        Loyal = 2,

        [Description("Tidak Aktif")]
        [Display(Name = "Tidak Aktif")]
        Inactive = 3,

        [Description("Berisiko")]
        [Display(Name = "Berisiko")]
        AtRisk = 4,

        [Description("Hilang")]
        [Display(Name = "Hilang")]
        Lost = 5,
    }
}
