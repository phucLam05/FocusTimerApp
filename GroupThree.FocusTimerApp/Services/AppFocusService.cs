using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.Services
{
    public class AppFocusService
    {
        public event Action<RegisteredAppModel>? EnteredWorkZone;
        public event Action<RegisteredAppModel>? LeftWorkZone;

        private readonly List<RegisteredAppModel> _registeredApps = new();
        private RegisteredAppModel? _currentFocusedApp;
        private readonly System.Timers.Timer _focusCheckTimer;

        public AppFocusService()
        {
            _focusCheckTimer = new System.Timers.Timer(1000); // check mỗi 1s
            _focusCheckTimer.Elapsed += CheckForeground;
            _focusCheckTimer.Start();
        }

        public IEnumerable<RegisteredAppModel> GetRegisteredApps() => _registeredApps;

        public void RegisterApp(RegisteredAppModel app)
        {
            if (!_registeredApps.Any(a => a.ExecutablePath == app.ExecutablePath))
                _registeredApps.Add(app);
        }

        public void UnregisterApp(string exePath)
        {
            var app = _registeredApps.FirstOrDefault(a => a.ExecutablePath == exePath);
            if (app != null)
                _registeredApps.Remove(app);
        }

        private void CheckForeground(object? sender, ElapsedEventArgs e)
        {
            try
            {
                IntPtr hWnd = GetForegroundWindow();
                if (hWnd == IntPtr.Zero) return;

                GetWindowThreadProcessId(hWnd, out uint pid);
                var proc = Process.GetProcessById((int)pid);
                string exePath = proc.MainModule?.FileName ?? string.Empty;

                var registered = _registeredApps.FirstOrDefault(a =>
                    exePath.Equals(a.ExecutablePath, StringComparison.OrdinalIgnoreCase));

                // 🟢 Nếu focus vào app khác hẳn so với trước đó
                if (registered != _currentFocusedApp)
                {
                    // Nếu rời khỏi app cũ
                    if (_currentFocusedApp != null)
                        LeftWorkZone?.Invoke(_currentFocusedApp);

                    // Nếu app mới thuộc vùng focus
                    if (registered != null)
                    {
                        _currentFocusedApp = registered;
                        _currentFocusedApp.LastActive = DateTime.Now;
                        EnteredWorkZone?.Invoke(registered);
                    }
                    else
                    {
                        _currentFocusedApp = null;
                    }
                }
            }
            catch
            {
                // bỏ qua lỗi nhỏ (system process, access denied,...)
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
