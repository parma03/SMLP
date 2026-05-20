using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.PointAdjustments.Queries;

public class GetPointAdjustmentRequest
{
    public Guid? CustomerId { get; set; }
    public AdjustmentType? AdjustmentType { get; set; }
    public DateTimeOffset? DateFrom { get; set; }
    public DateTimeOffset? DateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

