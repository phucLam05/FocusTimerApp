using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly HashSet<string> _shownWelcomeApps = new(StringComparer.OrdinalIgnoreCase);

        private readonly System.Timers.Timer _focusCheckTimer;
        private readonly object _lock = new();

        private bool _isInWorkZone = false;
        private RegisteredAppModel? _currentFocusedApp;
        private string? _lastExePathNormalized;

        // Thời gian ngưỡng (ms) để coi là "rời thật sự" khi focus ra ngoài (tránh bật tắt rất nhanh)
        private const int LeaveThresholdMs = 700;
        private DateTime? _leftCandidateAt = null;

        public AppFocusService()
        {
            _focusCheckTimer = new System.Timers.Timer(500); // kiểm tra 500ms cho responsiveness tốt
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
                    _registeredApps.Add(app);
            }
        }

        public void UnregisterApp(string exePath)
        {
            if (string.IsNullOrEmpty(exePath)) return;
            var norm = NormalizePath(exePath);
            lock (_lock)
            {
                var app = _registeredApps.FirstOrDefault(a => NormalizePath(a.ExecutablePath) == norm);
                if (app != null) _registeredApps.Remove(app);
                _shownWelcomeApps.Remove(norm);
            }
        }

        private void CheckForeground(object? sender, ElapsedEventArgs e)
        {
            try
            {
                string? exePath = GetForegroundAppPath();
                if (string.IsNullOrEmpty(exePath))
                {
                    // If no foreground, treat as outside
                    HandleOutside(null);
                    return;
                }

                string norm = NormalizePath(exePath);

                RegisteredAppModel? focusedRegistered;
                lock (_lock)
                {
                    focusedRegistered = _registeredApps
                        .FirstOrDefault(a => NormalizePath(a.ExecutablePath) == norm);
                }

                if (focusedRegistered != null)
                {
                    // We're focusing on a registered app (in work zone)
                    // reset left-candidate because we're in-zone now
                    _leftCandidateAt = null;

                    if (!_isInWorkZone)
                    {
                        // We were outside, now entered work zone -> start new session
                        _isInWorkZone = true;
                        _currentFocusedApp = focusedRegistered;

                        // Clear shown list only when entering after being outside
                        // (this ensures apps can be re-notified in a new session)
                        _shownWelcomeApps.Clear();

                        // Notify if not shown before in this new session
                        if (!_shownWelcomeApps.Contains(norm))
                        {
                            _shownWelcomeApps.Add(norm);
                            EnteredWorkZone?.Invoke(focusedRegistered);
                        }
                    }
                    else
                    {
                        // already in work zone
                        if (_currentFocusedApp == null || NormalizePath(_currentFocusedApp.ExecutablePath) != norm)
                        {
                            // switched to a different registered app
                            _currentFocusedApp = focusedRegistered;

                            if (!_shownWelcomeApps.Contains(norm))
                            {
                                _shownWelcomeApps.Add(norm);
                                EnteredWorkZone?.Invoke(focusedRegistered);
                            }
                        }
                        // else focusing same registered app -> nothing to do
                    }

                    _lastExePathNormalized = norm;
                }
                else
                {
                    // Foreground is NOT a registered app -> candidate to leave
                    HandleOutside(norm);
                    _lastExePathNormalized = norm;
                }
            }
            catch
            {
                // swallow (access denied etc.)
            }
        }

        // Handles being outside registered apps, with a small threshold to avoid spurious leave
        private void HandleOutside(string? currentNorm)
        {
            // If we were not in work zone already, nothing to do
            if (!_isInWorkZone) return;

            // If we just detected outside, mark candidate time
            if (_leftCandidateAt == null)
            {
                _leftCandidateAt = DateTime.UtcNow;
                return;
            }

            // If threshold not elapsed yet, wait
            if ((DateTime.UtcNow - _leftCandidateAt.Value).TotalMilliseconds < LeaveThresholdMs)
                return;

            // After threshold elapsed -> confirm left
            var lastApp = _currentFocusedApp;
            _currentFocusedApp = null;
            _isInWorkZone = false;
            _leftCandidateAt = null;

            // clear shown list because new session will start when re-enter
            _shownWelcomeApps.Clear();

            if (lastApp != null)
                LeftWorkZone?.Invoke(lastApp);
        }

        // Normalize path to avoid differences in case, relative vs absolute, short vs long paths.
        private static string NormalizePath(string? path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            try
            {
                // GetFullPath and lower-case to normalize.
                var full = Path.GetFullPath(path);
                return full.ToLowerInvariant();
            }
            catch
            {
                try { return path.ToLowerInvariant(); } catch { return path ?? string.Empty; }
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
