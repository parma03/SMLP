using System.ComponentModel;

namespace Smlp.Base.Transaction.Enums
{
    public enum TransactionStatus
    {
        [Description("Pending")] Pending = 1,
        [Description("Selesai")] Completed = 2,
        [Description("Dibatalkan")] Cancelled = 3,
    }
}
