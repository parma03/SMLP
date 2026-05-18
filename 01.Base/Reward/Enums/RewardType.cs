using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.Reward.Enums
{
    public enum RewardType
    {
        [Description("Produk Gratis")]
        [Display(Name = "Produk Gratis")]
        FreeItem = 0,

        [Description("Diskon Nominal")]
        [Display(Name = "Diskon Nominal")]
        DiscountAmount = 1,

        [Description("Diskon Persen")]
        [Display(Name = "Diskon Persen")]
        DiscountPercent = 2,

        [Description("Voucher")]
        [Display(Name = "Voucher")]
        Voucher = 3,
    }
}
