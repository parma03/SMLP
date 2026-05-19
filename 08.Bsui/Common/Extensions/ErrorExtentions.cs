using Smlp.Shared.Common.Responses;

namespace Smlp.Bsui.Common.Extensions;

public static class ErrorExtentions
{
    public static string ToDetail(this ErrorResponse err)
    {
        return string.Join(",", err.Details.ToList());
    }
}
