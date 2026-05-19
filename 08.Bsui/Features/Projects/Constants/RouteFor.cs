namespace Psmp.Bsui.Features.Projects.Constants;

public static class RouteFor
{
    public static readonly string Project = $"{nameof(Project)}";
    public const string EditProjectProposal = nameof(EditProjectProposal);
    public static string EditProjectProposaled(string id)
    {
        return $"{nameof(EditProjectProposal)}/{id}";
    }

    public const string DetailProjectMentoring = nameof(DetailProjectMentoring);
    public static string DetailProjectMentoringed(string id)
    {
        return $"{nameof(DetailProjectMentoring)}/{id}";
    }

    public const string EditProjectPlan = nameof(EditProjectPlan);
    public static string EditProjectPlaned(string id)
    {
        return $"{nameof(EditProjectPlan)}/{id}";
    }

    public const string EditProjectReal = nameof(EditProjectReal);
    public static string EditProjectRealed(string id)
    {
        return $"{nameof(EditProjectReal)}/{id}";
    }
}

