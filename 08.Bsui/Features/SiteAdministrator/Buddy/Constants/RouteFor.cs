namespace Smlp.Bsui.Features.SiteAdministrator.Budy.Constants;

public static class RouteFor
{
    public static readonly string SiteAdminstrator = $"{nameof(SiteAdminstrator)}";
    public static readonly string Buddy = $"{nameof(Buddy)}";

    public static string ApplicantPaymentd(string employeeNumber, string action)
    {
        return $"{SiteAdminstrator}/{employeeNumber}/{action}";
    }

}
