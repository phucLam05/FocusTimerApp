using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Commands;
using System.Threading.Tasks;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class AppControlViewModel : ViewModelBase
    {
        private readonly AppFocusService _focusService;
        private readonly TimerService _timerService;
        // private System.Windows.Forms.NotifyIcon? _notifyIcon;

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
            RefreshRunningCommand = new RelayCommand(LoadRunningApps);
            SaveCommand = new RelayCommand(SaveChanges);

            // Initialize notification icon for showing messages
            // InitializeNotifyIcon();

            LoadRunningApps();
            LoadFocusZoneApps();
        }

        // private void InitializeNotifyIcon()
        // {
        //     try
        //     {
        //         _notifyIcon = new System.Windows.Forms.NotifyIcon
        //         {
        //             Visible = false, // Only show when needed
        //             Icon = System.Drawing.SystemIcons.Application
        //         };
        //     }
        //     catch { }
        // }

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
                            ExecutablePath = exePath,
                            ProcessName = proc.ProcessName,
                            IsRunning = true
                        });
                    }
                }
                catch { /* Skip processes we can't access */ }
            }
        }

        private void LoadFocusZoneApps()
        {
            FocusZoneApps.Clear();
            var registeredApps = _focusService.GetRegisteredApps();

            foreach (var app in registeredApps)
            {
                if (!string.IsNullOrEmpty(app.ProcessName))
                {
                    FocusZoneApps.Add(app.ProcessName);
                }
            }
        }

        private void AddApp(RegisteredAppModel? app)
        {
            if (app == null) return;

            if (!FocusZoneApps.Contains(app.ProcessName))
            {
                _focusService.RegisterApp(app);
                FocusZoneApps.Add(app.ProcessName);
                RunningApps.Remove(app);
            }
        }

        private void RemoveApp(string? appName)
        {
            if (string.IsNullOrEmpty(appName)) return;

            var registeredApp = _focusService.GetRegisteredApps()
                .FirstOrDefault(a => a.ProcessName.Equals(appName, StringComparison.OrdinalIgnoreCase));

            if (registeredApp != null)
            {
                _focusService.UnregisterApp(registeredApp.ExecutablePath);
                FocusZoneApps.Remove(appName);
                LoadRunningApps(); // Refresh to show it in running apps again
            }
        }

        private void SaveChanges()
        {
            // Settings are auto-saved when adding/removing apps
            // Show success dialog to user
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
