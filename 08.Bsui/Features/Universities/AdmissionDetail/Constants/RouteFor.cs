namespace Smlp.Bsui.Features.Universities.AdmissionDetail.Constants;

public static class RouteFor
{
    public static readonly string UniversityRegistration = $"{nameof(UniversityRegistration)}";
    public static readonly string UniversityAdmissionDetail = $"{nameof(UniversityAdmissionDetail)}";
    public static string Detail(Guid id)
    {
        return $"{UniversityRegistration}/{UniversityAdmissionDetail}/{id}";
    }
}
