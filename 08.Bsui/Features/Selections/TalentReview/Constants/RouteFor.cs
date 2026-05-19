namespace Psmp.Bsui.Features.Selections.TalentReview.Constants;

public static class RouteFor
{
    public static readonly string Selection = $"{nameof(Selection)}";
    public static readonly string TalentReview = $"{nameof(TalentReview)}";
    public static readonly string Detail = $"{Selection}/{nameof(TalentReview)}/{nameof(Details)}";
    public static string Selections()
    {
        return $"{Selection}";
    }
    public static string Details(Guid id)
    {
        return $"{Detail}/{id}";
    }
}
