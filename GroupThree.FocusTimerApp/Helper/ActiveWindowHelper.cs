using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GroupThree.FocusTimerApp.Helpers
{
    public static class ActiveWindowHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static string? GetActiveProcessName()
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero) return null;

            GetWindowThreadProcessId(handle, out uint pid);
            try
            {
                Process p = Process.GetProcessById((int)pid);
                return p.ProcessName;
            }
            catch
            {
                return null;
            }
        }
    }
}
