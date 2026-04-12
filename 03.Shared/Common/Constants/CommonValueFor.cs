using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Smlp.Shared.Common.Constants;

public static class CommonValueFor
{
    public static readonly string OperatingSystemDescription = RuntimeInformation.OSDescription;

    public static Assembly EntryAssembly
    {
        get
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly is null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            return assembly;
        }
    }

    public static string EnvironmentName
    {
        get
        {
            var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariables.AspNetCoreEnvironment);

            if (environmentName is null)
            {
                environmentName = EnvironmentNames.Local;
            }

            return environmentName;
        }
    }

    public static readonly AssemblyName EntryAssemblyName = EntryAssembly.GetName();
    public static readonly Version? EntryAssemblyVersion = EntryAssemblyName.Version;
    public static readonly string? EntryAssemblySimpleName = EntryAssemblyName.Name;
    public static readonly string? EntryAssemblyInformationalVersion = EntryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    public static readonly string? EntryAssemblyFrameworkName = EntryAssembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
    public static readonly DateTime EntryAssemblyLastBuild = File.GetLastWriteTime(EntryAssembly.Location);
}
