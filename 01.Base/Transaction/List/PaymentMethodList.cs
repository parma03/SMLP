using Smlp.Base.Transaction.Enums;

namespace Smlp.Base.Transaction.List
{
    public static class PaymentMethodList
    {
        public static IList<PaymentMethod> GetPaymentMethods()
        {
            var list = new List<PaymentMethod>
            {
                PaymentMethod.Tunai,
                PaymentMethod.Qris,
                PaymentMethod.TransferBank,
                PaymentMethod.EWallet,
                PaymentMethod.Debit,
                PaymentMethod.Kredit
            };
            return list;
        }
    }
}
