using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMLP.Base.Common
{
    public enum CurrencyType
    {
        [Description("IDR")]
        [Display(Name = "IDR")]
        Idr = 1,

        [Description("USD")]
        [Display(Name = "USD")]
        Usd = 2,
    }

}
