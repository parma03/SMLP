using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.Customer.Enums
{
    public enum CustomerSegment
    {
        [Description("New")]
        [Display(Name = "New")]
        New = 0,

        [Description("Active")]
        [Display(Name = "Active")]
        Active = 1,

        [Description("Loyal")]
        [Display(Name = "Loyal")]
        Loyal = 2,

        [Description("Berisiko")]
        [Display(Name = "Berisiko")]
        AtRisk = 4,

        [Description("Inactive")]
        [Display(Name = "Inactive")]
        Inactive = 5,
    }
}
