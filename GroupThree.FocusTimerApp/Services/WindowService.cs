namespace GroupThree.FocusTimerApp.Services
{
    using System;
    using System.Windows;
    using GroupThree.FocusTimerApp.Views;
    using GroupThree.FocusTimerApp.ViewModels;

    /// <summary>
    /// Service for managing application windows
    /// Provides methods to show and manage different window types
    /// </summary>
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Shows the Settings window as a modal dialog
        /// Resolves window from DI container or creates manually if needed
        /// </summary>
        public void ShowSettingsWindow()
        {
            try
            {
                var app = Application.Current as App;
                HotkeyService? hotkey = app?.HotkeyServiceInstance;

                // Try to resolve SettingsWindow via DI
                var window = (SettingsWindow?)_serviceProvider.GetService(typeof(SettingsWindow));
                if (window == null)
                {
                    // Fallback: create settings VM and window manually
                    var settingsService = (SettingsService?)_serviceProvider.GetService(typeof(SettingsService)) ?? new SettingsService();
                    var themeService = (IThemeService?)_serviceProvider.GetService(typeof(IThemeService)) ?? new ThemeService();
                    var timerService = (TimerService?)_serviceProvider.GetService(typeof(TimerService));
                    var notificationService = (INotificationService?)_serviceProvider.GetService(typeof(INotificationService));
                    
                    var vm = new SettingsViewModel(settingsService, hotkey, themeService, timerService, notificationService);
                    window = new SettingsWindow();
                    window.DataContext = vm;
                }

                window.Owner = Application.Current?.MainWindow;
                window.ShowDialog();
                
                System.Diagnostics.Debug.WriteLine("[WindowService] Settings window shown");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WindowService] Error showing settings window: {ex.Message}");
            }
        }
    }
}
