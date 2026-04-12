using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Formats;

namespace Smlp.Shared.Common.Extensions;

public static class DateTimeOffsetExtensions
{
    public static string ToShortDateDisplayText(this DateTimeOffset dateTimeOffest)
    {
        return dateTimeOffest.LocalDateTime.ToString(DateTimeFormats.DD_MMM_YYYY);
    }

    public static string ToShortDateDisplayText(this DateTimeOffset? dateTimeOffest)
    {
        if (!dateTimeOffest.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTimeOffest.Value.LocalDateTime.ToString(DateTimeFormats.DD_MMM_YYYY);
    }

    public static string ToShortDateTimeDisplayText(this DateTimeOffset dateTimeOffest)
    {
        return dateTimeOffest.LocalDateTime.ToString(DateTimeFormats.DD_MMM_YYYY_HH_MM_SS);
    }

    public static string ToShortDateTimeDisplayText(this DateTimeOffset? dateTimeOffest)
    {
        if (!dateTimeOffest.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTimeOffest.Value.LocalDateTime.ToString(DateTimeFormats.DD_MMM_YYYY_HH_MM_SS);
    }

    public static string ToLongDateDisplayText(this DateTimeOffset dateTimeOffest)
    {
        return dateTimeOffest.LocalDateTime.ToString(DateTimeFormats.DD_MMMM_YYYY);
    }

    public static string ToLongDateDisplayText(this DateTimeOffset? dateTimeOffest)
    {
        if (!dateTimeOffest.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTimeOffest.Value.LocalDateTime.ToString(DateTimeFormats.DD_MMMM_YYYY);
    }

    public static string ToLongDateTimeDisplayText(this DateTimeOffset dateTimeOffest)
    {
        return dateTimeOffest.LocalDateTime.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS);
    }

    public static string ToLongDateTimeDisplayText(this DateTimeOffset? dateTimeOffest)
    {
        if (!dateTimeOffest.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTimeOffest.Value.LocalDateTime.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS);
    }

    public static string ToCompleteDateTimeDisplayText(this DateTimeOffset dateTimeOffest)
    {
        return dateTimeOffest.LocalDateTime.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS_ZZZ);
    }

    public static string ToCompleteDateTimeDisplayText(this DateTimeOffset? dateTimeOffest)
    {
        if (!dateTimeOffest.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTimeOffest.Value.LocalDateTime.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS_ZZZ);
    }

    public static string ToFriendlyTimeDisplayText(this DateTimeOffset dateTimeOffest)
    {
        var hour = dateTimeOffest.Hour;

        return hour is >= 4 and < 12 ?
            "Morning" : hour is >= 12 and < 17 ?
            "Afternoon" : hour is >= 17 and < 21 ?
            "Evening" : "Night";
    }
}
