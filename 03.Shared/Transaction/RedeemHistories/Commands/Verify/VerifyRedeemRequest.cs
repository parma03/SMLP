using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.RedeemHistories.Commands.Verify;

public class VerifyRedeemRequest
{
    public RedeemStatus Status { get; set; }
    public string? Notes { get; set; }
}
