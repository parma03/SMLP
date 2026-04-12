namespace Smlp.Shared.Common.Extensions;
public static class DecimalExtensions
{
    public static string ToCurrency(this decimal? val)
    {
        if (val == null)
        {
            return "0";
        }

        return $"{val:N}";
    }
}
