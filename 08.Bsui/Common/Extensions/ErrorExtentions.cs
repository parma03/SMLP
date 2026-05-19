using Psmp.Shared.Common.Responses;

namespace Psmp.Bsui.Common.Extensions;

public static class ErrorExtentions
{
    public static string ToDetail(this ErrorResponse err)
    {
        return string.Join(",", err.Details.ToList());
    }
}
