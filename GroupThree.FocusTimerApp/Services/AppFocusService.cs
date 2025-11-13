using System.Diagnostics;
using System.Runtime.InteropServices;
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
        private readonly SettingsService _settingsService;

        public AppFocusService(SettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // Load from settings
            LoadRegisteredAppsFromSettings();

            // Subscribe to settings changes to keep in sync
            _settingsService.SettingsChanged += _ => LoadRegisteredAppsFromSettings();

            // Khởi động timer kiểm tra app foreground
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
                // Kiểm tra app đã tồn tại chưa
                if (_registeredApps.Any(a => NormalizePath(a.ExecutablePath) == NormalizePath(app.ExecutablePath)))
                {
                    return;
                }

                // Validate executable exists
                if (!string.IsNullOrEmpty(app.ExecutablePath))
                {
                    try
                    {
                        if (!System.IO.File.Exists(app.ExecutablePath))
                        {
                            Debug.WriteLine($"[AppFocusService] Warning: Executable not found: {app.ExecutablePath}");
                            // Still add it, user might have the file later
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[AppFocusService] Cannot validate path: {ex.Message}");
                    }
                }

                _registeredApps.Add(app);
                PersistToSettings();
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
                    PersistToSettings();
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

        private void PersistToSettings()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                cfg.FocusApps = _registeredApps.ToList();
                _settingsService.SaveSettings(cfg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppFocusService] Persist failed: {ex.Message}");
            }
        }

        private void LoadRegisteredAppsFromSettings()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                var apps = cfg.FocusApps ?? new List<RegisteredAppModel>();

                lock (_lock)
                {
                    _registeredApps.Clear();

                    // Load tất cả apps có tên và đường dẫn hợp lệ
                    foreach (var app in apps)
                    {
                        // Bỏ qua app không có tên hoặc đường dẫn
                        if (string.IsNullOrWhiteSpace(app.AppName) &&
                            string.IsNullOrWhiteSpace(app.ExecutablePath))
                        {
                            Debug.WriteLine("[AppFocusService] Skipping invalid app entry (no name or path)");
                            continue;
                        }

                        // Validate executable exists (warning only, không block)
                        if (!string.IsNullOrEmpty(app.ExecutablePath))
                        {
                            try
                            {
                                if (!System.IO.File.Exists(app.ExecutablePath))
                                {
                                    Debug.WriteLine($"[AppFocusService] Warning: Registered app executable not found: {app.ExecutablePath}");
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"[AppFocusService] Cannot validate path for {app.AppName}: {ex.Message}");
                                continue;
                            }
                        }

                        _registeredApps.Add(app);
                    }
                }

                Debug.WriteLine($"[AppFocusService] Loaded {_registeredApps.Count} registered apps from settings");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppFocusService] Load from settings failed: {ex.Message}");
            }
        }

        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            try
            {
                return System.IO.Path.GetFullPath(path).ToLowerInvariant();
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
