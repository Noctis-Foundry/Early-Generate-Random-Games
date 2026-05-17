using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Win32;
using ValveKeyValue;

namespace SteamAppInfoParser;

class Program
{
    static int Main(string[] args)
    {
        var steamLocation = GetSteamPath();

        if (steamLocation == null)
        {
            Console.Error.WriteLine("Can not find Steam");
            return 1;
        }

        using (var stream = File.OpenRead(Path.Join(steamLocation, "appcache", "appinfo.vdf")))
        {
            var appInfo = DumpAppInfo(stream);

            var list = new List<uint>();

            foreach (var info in appInfo.Apps)
            {
                list.Add(info.AppID);
            }
            
            var json = JsonSerializer.Serialize(list);
            Console.WriteLine(json);
        }
        
        return 0;
    }

    private static AppInfo DumpAppInfo(FileStream inputStream)
    {
        var appInfo = new AppInfo();
        appInfo.Read(inputStream);

        return appInfo;
    }

    private static string? GetSteamPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam") ??
                      RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                          .OpenSubKey("SOFTWARE\\Valve\\Steam");

            if (key != null && key.GetValue("SteamPath") is string steamPath)
            {
                return steamPath;
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var paths = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" };

            return paths
                .Select(path => Path.Join(home, path))
                .FirstOrDefault(steamPath => Directory.Exists(Path.Join(steamPath, "appcache")));
        }
        else if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Join(home, "Steam");
        }

        throw new PlatformNotSupportedException();
    }
}
