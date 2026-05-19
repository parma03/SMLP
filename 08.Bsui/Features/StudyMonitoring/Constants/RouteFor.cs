namespace Psmp.Bsui.Features.StudyMonitoring.Constants;

public static class RouteFor
{
    public static readonly string StudyMonitoring = $"{nameof(StudyMonitoring)}";

    public static string StudyMonitoringTab(int tabIndex)
    {
        return $"{StudyMonitoring}/{tabIndex}";
    }

    public static string StudyMonitoringDetail(string page, Guid id)
    {
        return $"{StudyMonitoring}/{page}/{id}";
    }

    public static string StudyMonitoringDetailItem(string page, Guid id, Guid id2)
    {
        return $"{StudyMonitoring}/{page}/{id}/{id2}";
    }
}
