using System.ComponentModel;

namespace Smlp.Base.Transaction.Enums
{
    public enum PaymentMethod
    {
        [Description("Tunai")] Tunai = 1,
        [Description("QRIS")] Qris = 2,
        [Description("Transfer Bank")] TransferBank = 3,
        [Description("E-Wallet")] EWallet = 4,
        [Description("Kartu Debit")] Debit = 5,
        [Description("Kartu Kredit")] Kredit = 6,
    }
}
