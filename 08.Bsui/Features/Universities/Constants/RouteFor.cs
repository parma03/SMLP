namespace Psmp.Bsui.Features.Universities.Constants;

public static class RouteFor
{
    public static readonly string UniversityRegistration = $"{nameof(UniversityRegistration)}";

    public static string UniversityAdmissionTab(int idxTab)
    {
        return $"{UniversityRegistration}/{idxTab}";
    }
}
