namespace Smlp.Shared.Transaction.RedeemHistories.Commands.Create;

public class CreateRedeemRequest
{
    public Guid CustomerId { get; set; }
    public Guid RewardId { get; set; }
    public string? Notes { get; set; }
}
