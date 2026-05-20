using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.Selections.Enums
{
    public enum SelectionType
    {
        [Description("Single")]
        [Display(Name = "Single")]
        Single = 1,

        [Description("Multiple")]
        [Display(Name = "Multiple")]
        Multiple = 2,
    }
}
