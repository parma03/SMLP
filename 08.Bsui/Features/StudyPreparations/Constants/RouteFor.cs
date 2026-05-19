namespace Smlp.Bsui.Features.StudyPreparations.Constants;

public static class RouteFor
{
    public static readonly string StudyPreparation = $"{nameof(StudyPreparation)}";
    public static readonly string Detail = $"{nameof(Detail)}";
    public static readonly string DetailAccepted = $"{nameof(DetailAccepted)}";

    public static string StudyPreparationTab(int tabIndex)
    {
        return $"{StudyPreparation}/{tabIndex}";
    }

    public static string StudyPreparationDetail(string page, Guid tabIndex)
    {
        return $"{StudyPreparation}/{page}/{Detail}/{tabIndex}";
    }

    public static string StudyPreparationDetailAccepted(string page, Guid tabIndex)
    {
        return $"{StudyPreparation}/{page}/{DetailAccepted}/{tabIndex}";
    }
}

