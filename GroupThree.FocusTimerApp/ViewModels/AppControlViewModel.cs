using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Commands;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class AppControlViewModel : ViewModelBase
    {
        private readonly AppFocusService _focusService;
        private readonly TimerService _timerService;

        // Running apps list
        public ObservableCollection<RegisteredAppModel> RunningApps { get; } = new();

        // Focus Zone apps (blocked apps)
        public ObservableCollection<string> FocusZoneApps { get; } = new();

        public ICommand AddAppCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand RefreshRunningCommand { get; }
        public ICommand SaveCommand { get; }

        public AppControlViewModel(AppFocusService focusService, TimerService timerService)
        {
            _focusService = focusService;
            _timerService = timerService;

            AddAppCommand = new RelayCommand<RegisteredAppModel>(AddApp);
            RemoveAppCommand = new RelayCommand<string>(RemoveApp);
            RefreshRunningCommand = new RelayCommand<object>(_ => LoadRunningApps());
            SaveCommand = new RelayCommand<object>(_ => SaveChanges());

            LoadRunningApps();
            LoadFocusZoneApps();
        }

        private void LoadRunningApps()
        {
            RunningApps.Clear();

            int currentProcessId = Process.GetCurrentProcess().Id;
            string? currentProcessPath = Environment.ProcessPath;

            var processes = Process.GetProcesses()
                .Where(p => p.Id != currentProcessId)
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .DistinctBy(p => p.ProcessName);

            foreach (var proc in processes)
            {
                try
                {
                    string exePath = proc.MainModule?.FileName ?? string.Empty;

                    if (!string.IsNullOrEmpty(exePath))
                    {
                        // Skip our own process
                        if (!string.IsNullOrEmpty(currentProcessPath) &&
                            string.Equals(exePath, currentProcessPath, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Skip if already in focus zone
                        if (FocusZoneApps.Any(app => app.Equals(proc.ProcessName, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        RunningApps.Add(new RegisteredAppModel
                        {
                            AppName = proc.ProcessName,
                            ExecutablePath = exePath
                        });
                    }
                }
                catch { /* Skip processes can't access */ }
            }
        }

        private void LoadFocusZoneApps()
        {
            FocusZoneApps.Clear();

            try
            {
                var registeredApps = _focusService.GetRegisteredApps();

                foreach (var app in registeredApps)
                {
                    // Validate app có tên và đường dẫn hợp lệ
                    if (string.IsNullOrWhiteSpace(app.AppName))
                    {
                        continue;
                    }

                    string displayName = app.AppName;

                    // Validate executable tồn tại (optional warning)
                    if (!string.IsNullOrEmpty(app.ExecutablePath))
                    {
                        try
                        {
                            if (!System.IO.File.Exists(app.ExecutablePath))
                            {
                                Debug.WriteLine($"[AppControlViewModel] Warning: App '{displayName}' executable not found: {app.ExecutablePath}");
                                // Vẫn hiển thị app, cho phép user xóa nó
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[AppControlViewModel] Cannot validate '{displayName}': {ex.Message}");
                        }
                    }

                    if (!string.IsNullOrEmpty(displayName) && !FocusZoneApps.Contains(displayName))
                    {
                        FocusZoneApps.Add(displayName);
                    }
                }

                Debug.WriteLine($"[AppControlViewModel] Loaded {FocusZoneApps.Count} apps to Focus Zone display");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppControlViewModel] LoadFocusZoneApps error: {ex.Message}");
            }
        }

        private void AddApp(RegisteredAppModel? app)
        {
            if (app == null) return;

            try
            {
                // Validate app có đủ thông tin
                if (string.IsNullOrWhiteSpace(app.ExecutablePath))
                {
                    Debug.WriteLine("[AppControlViewModel] Cannot add app: No executable path");
                    return;
                }

                string displayName = app.AppName;

                if (!FocusZoneApps.Contains(displayName))
                {
                    _focusService.RegisterApp(app);
                    FocusZoneApps.Add(displayName);
                    RunningApps.Remove(app);

                    Debug.WriteLine($"[AppControlViewModel] Added app: {displayName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppControlViewModel] AddApp error: {ex.Message}");
            }
        }

        private void RemoveApp(string? appName)
        {
            if (string.IsNullOrEmpty(appName)) return;

            try
            {
                var registeredApp = _focusService.GetRegisteredApps()
                    .FirstOrDefault(a => a.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase));

                if (registeredApp != null)
                {
                    _focusService.UnregisterApp(registeredApp.ExecutablePath);
                    FocusZoneApps.Remove(appName);
                    LoadRunningApps(); // Refresh to show it in running apps again

                    Debug.WriteLine($"[AppControlViewModel] Removed app: {appName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppControlViewModel] RemoveApp error: {ex.Message}");
            }
        }

        private void SaveChanges()
        {
            // Settings are auto-saved when adding/removing apps
            ShowSuccessDialog("Settings Saved", "Focus Zone apps settings have been saved successfully!");
        }

        private void ShowSuccessDialog(string title, string message)
        {
            try
            {
                var dialog = new Views.SuccessDialog(title, message)
                {
                    Owner = System.Windows.Application.Current?.MainWindow
                };
                dialog.ShowDialog();
            }
            catch { }
        }
    }
}
