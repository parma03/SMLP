namespace Smlp.Bsui.Common.Helper;

public static class SetDataStatusHelper
{

    public static bool IsRequestIdDisabled(DateOnly? disabledFrom, DateOnly? disabledTo)
    {
        return !IsWithinApprovalWindow(disabledFrom, disabledTo);
    }

    public static bool IsApprovalDisabled(DateOnly? disabledFrom, DateOnly? disabledTo)
    {
        return IsWithinApprovalWindow(disabledFrom, disabledTo);
    }

    private static bool IsWithinApprovalWindow(DateOnly? disabledFrom = null, DateOnly? disabledTo = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var to = disabledTo ?? DateOnly.MaxValue;
        var from = disabledFrom ?? DateOnly.MinValue;

        return today.Day >= from.Day && today.Day <= to.Day;
    }
}
