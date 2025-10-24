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
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private readonly System.Timers.Timer _checkTimer;
        private readonly List<RegisteredAppModel> _registeredApps = new();
        private RegisteredAppModel? _currentActiveApp;

        public event Action<RegisteredAppModel>? EnteredWorkZone;
        public event Action<RegisteredAppModel>? LeftWorkZone;

        public AppFocusService()
        {
            _checkTimer = new System.Timers.Timer(1000); // check mỗi giây
            _checkTimer.Elapsed += (_, __) => CheckForeground();
        }

        public void Start() => _checkTimer.Start();
        public void Stop() => _checkTimer.Stop();

        // ===============================
        // 🧠 1. QUẢN LÝ APP ĐƯỢC ĐĂNG KÝ
        // ===============================
        public void RegisterApp(RegisteredAppModel app)
        {
            if (!_registeredApps.Any(a => a.ExecutablePath.Equals(app.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                _registeredApps.Add(app);
        }

        public void UnregisterApp(string exePath)
        {
            _registeredApps.RemoveAll(a => a.ExecutablePath.Equals(exePath, StringComparison.OrdinalIgnoreCase));
        }

        public IReadOnlyList<RegisteredAppModel> GetRegisteredApps() => _registeredApps.AsReadOnly();

        // ===============================
        // 🧭 2. THEO DÕI APP ĐANG FOCUS
        // ===============================
        private void CheckForeground()
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

                if (registered != null && _currentActiveApp == null)
                {
                    _currentActiveApp = registered;
                    _currentActiveApp.LastActive = DateTime.Now;
                    EnteredWorkZone?.Invoke(registered);
                }
                else if (registered == null && _currentActiveApp != null)
                {
                    var left = _currentActiveApp;
                    _currentActiveApp = null;
                    LeftWorkZone?.Invoke(left);
                }
            }
            catch
            {
                // bỏ qua lỗi nhỏ (system process, access denied,...)
            }
        }

        // ============================================
        // 🔍 3. QUÉT DANH SÁCH ỨNG DỤNG ĐANG CHẠY
        // ============================================
        public IReadOnlyList<RegisteredAppModel> GetRunningApps()
        {
            var apps = new List<RegisteredAppModel>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    // chỉ lấy process có cửa sổ thực
                    if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowHandle != IntPtr.Zero)
                    {
                        apps.Add(new RegisteredAppModel
                        {
                            AppName = process.ProcessName,
                            ExecutablePath = process.MainModule?.FileName ?? "Unknown",
                            LastActive = DateTime.Now,
                            IsRunning = true
                        });
                    }
                }
                catch
                {
                    // bỏ qua process không truy cập được
                }
            }

            // sắp xếp theo tên cho dễ nhìn
            return apps.OrderBy(a => a.AppName).ToList();
        }
    }
}
