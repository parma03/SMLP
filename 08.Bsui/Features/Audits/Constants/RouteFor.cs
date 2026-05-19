namespace Psmp.Bsui.Features.Audits.Constants;

public static class RouteFor
{
    public const string Index = nameof(Audits);

    public static string Details(Guid id)
    {
        return $"{nameof(Audits)}/{nameof(Details)}/{id}";
    }
}
