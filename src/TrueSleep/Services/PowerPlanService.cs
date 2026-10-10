using System.Diagnostics;
using System.IO;

namespace TrueSleep.Services;

internal static class PowerPlanService
{
    public const string EnableHibernateArgument = "--enable-hibernate";
    public const string ConfigureLidArgument = "--configure-lid-hibernate";

    public static async Task<int> RunHelperAsync(string argument)
    {
        return argument switch
        {
            EnableHibernateArgument => await EnableHibernateAsync().ConfigureAwait(false),
            ConfigureLidArgument => await ConfigureLidHibernateAsync().ConfigureAwait(false),
            _ => 2,
        };
    }

    public static async Task<bool> RunElevatedHelperAsync(string argument)
    {
        string executable = Environment.ProcessPath ?? throw new InvalidOperationException("Executable path is unavailable.");
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = argument,
            UseShellExecute = true,
            Verb = "runas",
        };

        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync().ConfigureAwait(false);
        return process.ExitCode == 0;
    }

    private static async Task<int> EnableHibernateAsync() =>
        await RunPowerCfgAsync("/hibernate", "on").ConfigureAwait(false);

    private static async Task<int> ConfigureLidHibernateAsync()
    {
        int result = await EnableHibernateAsync().ConfigureAwait(false);
        if (result != 0)
        {
            return result;
        }

        result = await RunPowerCfgAsync(
            "/setacvalueindex",
            "SCHEME_CURRENT",
            "SUB_BUTTONS",
            "LIDACTION",
            "2").ConfigureAwait(false);
        if (result != 0)
        {
            return result;
        }

        result = await RunPowerCfgAsync(
            "/setdcvalueindex",
            "SCHEME_CURRENT",
            "SUB_BUTTONS",
            "LIDACTION",
            "2").ConfigureAwait(false);
        if (result != 0)
        {
            return result;
        }

        return await RunPowerCfgAsync("/setactive", "SCHEME_CURRENT").ConfigureAwait(false);
    }

    private static async Task<int> RunPowerCfgAsync(params string[] arguments)
    {
        string systemDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(systemDirectory, "powercfg.exe"),
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            return 1;
        }

        await process.WaitForExitAsync().ConfigureAwait(false);
        return process.ExitCode;
    }
}
