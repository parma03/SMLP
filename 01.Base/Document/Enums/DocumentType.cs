using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Smlp.Base.Document.Enums
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
