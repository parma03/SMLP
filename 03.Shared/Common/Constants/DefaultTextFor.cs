/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

namespace Smlp.Shared.Common.Constants;

public static class DefaultTextFor
{
    public static readonly string NA = "N/A";
    public static readonly string Dash = "-";
    public static readonly string Comma = ",";
    public static readonly string Unknown = nameof(Unknown);
    public static readonly string SystemBackgroundJob = nameof(SystemBackgroundJob);

    public static string IsEmptyFor(string text)
    {
        return $"{text} is Empty";
    }
}
