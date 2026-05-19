using MudBlazor;

namespace Smlp.Bsui.Features.LoyaltyAnalysis;

public partial class Index
{
    private List<TierStat> _tierStats = new()
    {
        new("Bronze", Color.Default),
        new("Silver", Color.Default),
        new("Gold", Color.Warning),
        new("Platinum", Color.Primary),
    };

    public record TierStat(string Name, Color Color);
}
