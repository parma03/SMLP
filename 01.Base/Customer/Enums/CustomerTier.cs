using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.Customer.Enums
{
    public enum CustomerTier
    {
        [Description("Bronze")]
        [Display(Name = "Bronze")]
        Bronze = 0,

        [Description("Silver")]
        [Display(Name = "Silver")]
        Silver = 1,

        [Description("Gold")]
        [Display(Name = "Gold")]
        Gold = 2,

        [Description("Platinum")]
        [Display(Name = "Platinum")]
        Platinum = 3,
    }
}
