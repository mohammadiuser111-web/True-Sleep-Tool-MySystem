using System.Runtime.InteropServices;

namespace TrueSleep.Interop;

internal static class PowerNative
{
    [DllImport("powrprof.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetSuspendState(
        [MarshalAs(UnmanagedType.Bool)] bool hibernate,
        [MarshalAs(UnmanagedType.Bool)] bool forceCritical,
        [MarshalAs(UnmanagedType.Bool)] bool disableWakeEvent);

    [DllImport("powrprof.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsPwrHibernateAllowed();

    [DllImport("powrprof.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool IsPwrSuspendAllowed();
}
