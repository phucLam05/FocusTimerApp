using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Timers;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.Services
{
    public class AppFocusService
    {
        public event Action? EnteredWorkZone;
        public event Action? LeftWorkZone;

        private readonly List<RegisteredAppModel> _registeredApps = new();
        private readonly object _lock = new();

        private bool _isInWorkZone = false;
        private DateTime? _leftCandidateAt = null;
        private const int LeaveThresholdMs = 700;

        private readonly System.Timers.Timer _focusCheckTimer;
        private readonly string _saveFilePath;

        public AppFocusService()
        {
            // ✅ setup file JSON
            string appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GroupThree.FocusTimerApp");
            Directory.CreateDirectory(appDataDir);
            _saveFilePath = Path.Combine(appDataDir, "registered_apps.json");

            LoadRegisteredApps();

            // ✅ khởi động timer kiểm tra app foreground
            _focusCheckTimer = new System.Timers.Timer(500);
            _focusCheckTimer.Elapsed += CheckForeground;
            _focusCheckTimer.AutoReset = true;
            _focusCheckTimer.Start();
        }

        public IEnumerable<RegisteredAppModel> GetRegisteredApps() => _registeredApps;

        public void RegisterApp(RegisteredAppModel app)
        {
            if (app == null) return;
            lock (_lock)
            {
                if (!_registeredApps.Any(a => NormalizePath(a.ExecutablePath) == NormalizePath(app.ExecutablePath)))
                {
                    _registeredApps.Add(app);
                    SaveRegisteredApps();
                }
            }
        }

        public void UnregisterApp(string exePath)
        {
            if (string.IsNullOrEmpty(exePath)) return;
            var norm = NormalizePath(exePath);
            lock (_lock)
            {
                var app = _registeredApps.FirstOrDefault(a => NormalizePath(a.ExecutablePath) == norm);
                if (app != null)
                {
                    _registeredApps.Remove(app);
                    SaveRegisteredApps();
                }
            }
        }

        private void CheckForeground(object? sender, ElapsedEventArgs e)
        {
            try
            {
                string? exePath = GetForegroundAppPath();
                if (string.IsNullOrEmpty(exePath))
                {
                    HandleOutside();
                    return;
                }

                string norm = NormalizePath(exePath);
                bool isRegistered;

                lock (_lock)
                {
                    isRegistered = _registeredApps.Any(a => NormalizePath(a.ExecutablePath) == norm);
                }

                if (isRegistered)
                {
                    // Đang trong vùng làm việc
                    _leftCandidateAt = null;

                    if (!_isInWorkZone)
                    {
                        _isInWorkZone = true;
                        EnteredWorkZone?.Invoke();
                    }
                }
                else
                {
                    HandleOutside();
                }
            }
            catch
            {
                // bỏ qua lỗi truy cập process
            }
        }

        private void HandleOutside()
        {
            if (!_isInWorkZone) return;

            if (_leftCandidateAt == null)
            {
                _leftCandidateAt = DateTime.UtcNow;
                return;
            }

            if ((DateTime.UtcNow - _leftCandidateAt.Value).TotalMilliseconds < LeaveThresholdMs)
                return;

            // Xác nhận rời khỏi vùng làm việc
            _isInWorkZone = false;
            _leftCandidateAt = null;
            LeftWorkZone?.Invoke();
        }

        // 🔧 JSON persistence
        private void SaveRegisteredApps()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_registeredApps, options);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppFocusService] Save failed: {ex.Message}");
            }
        }

        private void LoadRegisteredApps()
        {
            try
            {
                if (!File.Exists(_saveFilePath)) return;
                var json = File.ReadAllText(_saveFilePath);
                var apps = JsonSerializer.Deserialize<List<RegisteredAppModel>>(json);
                if (apps != null)
                {
                    _registeredApps.Clear();
                    _registeredApps.AddRange(apps);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppFocusService] Load failed: {ex.Message}");
            }
        }

        // 🔧 utils
        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            try
            {
                return Path.GetFullPath(path).ToLowerInvariant();
            }
            catch
            {
                return path?.ToLowerInvariant() ?? string.Empty;
            }
        }

        private static string? GetForegroundAppPath()
        {
            try
            {
                IntPtr hWnd = GetForegroundWindow();
                if (hWnd == IntPtr.Zero) return null;

                _ = GetWindowThreadProcessId(hWnd, out uint pid);
                var process = Process.GetProcessById((int)pid);
                return process.MainModule?.FileName;
            }
            catch
            {
                return null;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
