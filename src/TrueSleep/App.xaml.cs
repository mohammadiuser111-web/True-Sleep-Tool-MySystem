using System.Threading;
using System.Windows;
using TrueSleep.Services;

namespace TrueSleep;

public partial class App : Application
{
    private static Mutex? _singleInstance;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Length == 1 && e.Args[0].StartsWith("--", StringComparison.Ordinal))
        {
            int exitCode = PowerPlanService.RunHelperAsync(e.Args[0]).GetAwaiter().GetResult();
            Shutdown(exitCode);
            return;
        }

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            MessageBox.Show(
                "True Sleep requires Windows 11.",
                "True Sleep Tool - MySystem",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        _singleInstance = new Mutex(
            initiallyOwned: true,
            "Local\\MySystem.TrueSleep.8E09C919",
            out bool isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown();
            return;
        }

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstance?.Dispose();
        _singleInstance = null;
        base.OnExit(e);
    }
}
