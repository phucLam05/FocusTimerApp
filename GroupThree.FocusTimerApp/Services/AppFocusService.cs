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

        // trạng thái chung: đang ở trong work zone hay không
        private bool _isInWorkZone = false;

        // danh sách các exePath đã được thông báo trong session hiện tại
        private readonly HashSet<string> _notifiedApps = new(StringComparer.OrdinalIgnoreCase);

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
            if (!_registeredApps.Any(a => a.ExecutablePath.Equals(app.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                _registeredApps.Add(app);
        }

        public void UnregisterApp(string exePath)
        {
            var app = _registeredApps.FirstOrDefault(a => a.ExecutablePath.Equals(exePath, StringComparison.OrdinalIgnoreCase));
            if (app != null)
                _registeredApps.Remove(app);

            // nếu bỏ đăng ký app đang nằm trong danh sách đã thông báo, cũng loại khỏi set
            _notifiedApps.Remove(exePath);
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

                // kiểm tra app hiện tại có nằm trong registered list không
                var registered = _registeredApps.FirstOrDefault(a =>
                    exePath.Equals(a.ExecutablePath, StringComparison.OrdinalIgnoreCase));

                if (registered != null)
                {
                    // đang focus 1 app thuộc work zone
                    // nếu trước đó đang ở ngoài vùng -> ta coi đây là "vào vùng làm việc" (một session mới)
                    if (!_isInWorkZone)
                    {
                        _isInWorkZone = true;
                        _currentFocusedApp = registered;
                        _currentFocusedApp.LastActive = DateTime.Now;

                        // trước khi notify app này, đảm bảo nó chưa được notify trong session hiện tại
                        if (!_notifiedApps.Contains(registered.ExecutablePath))
                        {
                            _notifiedApps.Add(registered.ExecutablePath);
                            EnteredWorkZone?.Invoke(registered);
                        }
                    }
                    else
                    {
                        // đã ở trong work zone rồi: update current focused app
                        _currentFocusedApp = registered;
                        _currentFocusedApp.LastActive = DateTime.Now;

                        // nếu app này chưa được thông báo trong session hiện tại -> thông báo
                        if (!_notifiedApps.Contains(registered.ExecutablePath))
                        {
                            _notifiedApps.Add(registered.ExecutablePath);
                            EnteredWorkZone?.Invoke(registered);
                        }
                        // nếu đã thông báo rồi -> không notify (giữ im lặng)
                    }
                }
                else
                {
                    // focus vào app không thuộc work zone
                    if (_isInWorkZone)
                    {
                        // rời khỏi vùng làm việc hoàn toàn -> trigger LeftWorkZone cho last app (nếu có)
                        var lastApp = _currentFocusedApp;
                        _currentFocusedApp = null;
                        _isInWorkZone = false;

                        // clear danh sách các app đã thông báo trong session trước đó
                        _notifiedApps.Clear();

                        if (lastApp != null)
                            LeftWorkZone?.Invoke(lastApp);
                    }
                    // else: vẫn đang ngoài vùng -> không làm gì
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
