namespace Smlp.Shared.Services.Authorization.Models.GetPositions;

public class GetPositionsResponse
{
    public IEnumerable<PositionItem> Positions { get; set; } = Enumerable.Empty<PositionItem>();
}

public class PositionItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
