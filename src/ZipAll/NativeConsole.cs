using System.Runtime.InteropServices;

namespace ZipAll;

internal static class NativeConsole
{
    private const int AttachParentProcess = -1;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);

    public static void AttachToParentConsole()
    {
        AttachConsole(AttachParentProcess);
    }
}
