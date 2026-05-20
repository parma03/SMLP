using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.PointAdjustments.Commands.Create;

public class CreatePointAdjustmentRequest
{
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string Reason { get; set; } = default!;
}
