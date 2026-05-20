using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Transaction.PointAdjustments.Queries;

public class GetPointAdjustmentResponse : Response
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerMemberCode { get; set; } = default!;
    public int Points { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public string Reason { get; set; } = default!;
    public string ApprovedBy { get; set; } = default!;
    public DateTimeOffset AdjustmentDate { get; set; }
}
