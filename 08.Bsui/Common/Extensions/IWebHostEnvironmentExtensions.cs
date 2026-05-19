namespace Smlp.Bsui.Common.Extensions;

public static class IWebHostEnvironmentExtensions
{
    public static bool IsInEnvironment(this IWebHostEnvironment webHostEnvironment, params string[] environmentNames)
    {
        var result = false;
        var currentEnvironmentName = webHostEnvironment.EnvironmentName;

        foreach (var environmentName in environmentNames)
        {
            result |= currentEnvironmentName == environmentName;
        }

        return result;
    }
}
