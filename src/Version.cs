using System.Reflection;

namespace src
{
    public static class Version
    {
        public static string Num()
        {
            string version = Assembly.GetEntryAssembly()
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "unknown";

            // The SDK appends '+<git commit hash>' to the informational version.
            int plusIndex = version.IndexOf('+');
            return plusIndex >= 0 ? version[..plusIndex] : version;
        }
    }
}
