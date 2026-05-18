using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.Customer.Enums
{
    public enum Gender
    {
        [Description("Laki-laki")]
        [Display(Name = "Laki-laki")]
        LakiLaki = 0,

        [Description("Perempuan")]
        [Display(Name = "Perempuan")]
        Perempuan = 1,
    }
}
