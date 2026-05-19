/*
*/

namespace Smlp.Bsui.Common.Constants;

public static class NotificationMessageFor
{
    public static string DataNotFound(string dataFor)
    {
        return $"Data {dataFor} Not Found.";
    }

    public static string CannotEmpty(string dataFor)
    {
        return $"Data {dataFor} Cannot be Empty.";
    }

    public static string MinimumTextRequired(string dataFor)
    {
        return $"Data {dataFor} Not Match with Minimum Length.";
    }

    public static readonly string NotAllowScheduleSet = "Your scholarship selection process not allowed to set schedule test detail.";
    public static readonly string NotInSelectionStage = "Your selection stage not define.";
}
