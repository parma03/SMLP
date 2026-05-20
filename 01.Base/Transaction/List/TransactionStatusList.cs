using Smlp.Base.Transaction.Enums;

namespace Smlp.Base.Transaction.List
{
    public static class TransactionStatusList
    {
        public static IList<TransactionStatus> GetTransactionStatuses()
        {
            var list = new List<TransactionStatus>
            {
                TransactionStatus.Pending,
                TransactionStatus.Completed,
                TransactionStatus.Cancelled
            };
            return list;
        }
    }
}
