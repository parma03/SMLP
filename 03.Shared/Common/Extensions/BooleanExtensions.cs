namespace Smlp.Shared.Common.Extensions;

public static class BooleanExtensions
{
    public static string ToYesNoDisplayText(this bool value)
    {
        return value ? "Yes" : "No";
    }

    public static string ToEnabledDisabledDisplayText(this bool value)
    {
        return value ? "Enabled" : "Disabled";
    }
}
