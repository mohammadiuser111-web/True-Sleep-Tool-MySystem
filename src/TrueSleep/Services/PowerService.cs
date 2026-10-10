using System.ComponentModel;
using System.Runtime.InteropServices;
using TrueSleep.Interop;

namespace TrueSleep.Services;

internal static class PowerService
{
    public static bool CanSleep => PowerNative.IsPwrSuspendAllowed();

    public static bool CanHibernate => PowerNative.IsPwrHibernateAllowed();

    public static async Task DeepSleepAsync()
    {
        if (!CanHibernate)
        {
            bool enabled = await PowerPlanService
                .RunElevatedHelperAsync(PowerPlanService.EnableHibernateArgument)
                .ConfigureAwait(true);
            if (!enabled || !CanHibernate)
            {
                throw new InvalidOperationException("Hibernate could not be enabled.");
            }
        }

        await Task.Delay(250).ConfigureAwait(true);
        if (!PowerNative.SetSuspendState(hibernate: true, forceCritical: false, disableWakeEvent: true))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not enter deep sleep.");
        }
    }

    public static async Task ForceSleepAsync()
    {
        if (!CanSleep)
        {
            throw new InvalidOperationException("Sleep is not available on this device.");
        }

        await Task.Delay(250).ConfigureAwait(true);
        if (!PowerNative.SetSuspendState(hibernate: false, forceCritical: false, disableWakeEvent: true))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not enter sleep.");
        }
    }

    public static Task<bool> ConfigureLidForDeepSleepAsync() =>
        PowerPlanService.RunElevatedHelperAsync(PowerPlanService.ConfigureLidArgument);
}
