using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SMLP.Base.User.Enums
{
    public enum DocumentType
    {
        [Description("Nota")]
        [Display(Name = "Nota")]
        Nota = 0,

        [Description("Laporan")]
        [Display(Name = "Laporan")]
        Laporan = 1,
    }
}
