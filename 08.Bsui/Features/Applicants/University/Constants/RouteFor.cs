namespace Psmp.Bsui.Features.Applicants.University.Constants;

public static class RouteFor
{
    public static readonly string ApplicantUniversity = $"{nameof(ApplicantUniversity)}";

    public static string ApplicantUnivesityTab(int tabIndex)
    {
        return $"{ApplicantUniversity}/{tabIndex}";
    }

    public static string ApplicantUnivesityDetail(string page, Guid id)
    {
        return $"{ApplicantUniversity}/{page}/{id}";
    }
}
