using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace GroupThree.FocusTimerApp.Services
{
    public class StartupService
    {
        private const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        private readonly string _appName;
        private readonly string _exePath;

        public StartupService(string? appName = null, string? exePath = null)
        {
            _appName = string.IsNullOrWhiteSpace(appName)
                ? (System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "FocusTimerApp")
                : appName;

            _exePath = !string.IsNullOrWhiteSpace(exePath)
                ? exePath
                : (Process.GetCurrentProcess().MainModule?.FileName ?? System.Reflection.Assembly.GetEntryAssembly()?.Location ?? string.Empty);
        }

        public void ApplyStartupSetting(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true) ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, true);
                if (key == null) return;

                if (enable)
                {
                    var path = _exePath;
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
                    // Quote path in case it contains spaces
                    key.SetValue(_appName, $"\"{path}\"");
                }
                else
                {
                    if (key.GetValue(_appName) != null)
                    {
                        key.DeleteValue(_appName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApplyStartupSetting error: {ex.Message}");
            }
        }

        public bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
                if (key == null) return false;
                return key.GetValue(_appName) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
