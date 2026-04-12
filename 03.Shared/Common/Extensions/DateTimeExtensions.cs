/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Globalization;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Formats;

namespace Smlp.Shared.Common.Extensions;

public static class DateTimeExtensions
{
    public static string ToShortDateDisplayText(this DateTime dateTime)
    {
        return dateTime.ToString(DateTimeFormats.DD_MMM_YYYY);
    }

    public static string ToShortDateDisplayText(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTime.Value.ToString(DateTimeFormats.DD_MMM_YYYY);
    }

    public static string ToLongDateDisplayText(this DateTime dateTime)
    {
        return dateTime.ToString(DateTimeFormats.DD_MMMM_YYYY);
    }

    public static string ToLongDateTimeDisplayText(this DateTime dateTime)
    {
        return dateTime.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS);
    }

    public static string ToLongDateTimeDisplayText(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTime.Value.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS);
    }

    public static string ToCompleteDateTimeDisplayText(this DateTime dateTime)
    {
        return dateTime.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS_ZZZ);
    }

    public static string ToCompleteDateTimeDisplayText(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return DefaultTextFor.Dash;
        }

        return dateTime.Value.ToString(DateTimeFormats.DD_MMMM_YYYY_HH_MM_SS_ZZZ);
    }

    public static DateTime ToDateTime(this string dateTime)
    {
        return DateTimeOffset.ParseExact(dateTime, new string[] {
            "MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy",
            "yyyy/MM/dd",
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "yyyy-MM-dd" }, CultureInfo.InvariantCulture).DateTime;
    }

    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }

    public static string ToLocalDisplayName(this DateTime dateTime)
    {
        string[] monthNames = {
                "Januari", "Februari", "Maret",
                "April", "Mei", "Juni", "Juli",
                "Agustus", "September", "Oktober",
            "November", "Desember"};

        var day = dateTime.Day;
        var month = dateTime.Month;
        var year = dateTime.Year;

        return string.Format("{0} {1} {2}", day, monthNames[(int)month - 1], year);
    }
}
