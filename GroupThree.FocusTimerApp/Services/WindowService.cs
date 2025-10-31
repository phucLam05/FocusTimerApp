namespace GroupThree.FocusTimerApp.Services
{
    using System;
    using System.Windows;
    using GroupThree.FocusTimerApp.Views;
    using GroupThree.FocusTimerApp.ViewModels;

    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void ShowSettingsWindow()
        {
            try
            {
                var app = Application.Current as App;
                HotkeyService? hotkey = app?.HotkeyServiceInstance;

                var settingsService =
                    (SettingsService?)_serviceProvider.GetService(typeof(SettingsService))
                    ?? new SettingsService();

                var focusService =
                    (AppFocusService?)_serviceProvider.GetService(typeof(AppFocusService))
                    ?? new AppFocusService(settingsService);

                var timerService =
                    (TimerService?)_serviceProvider.GetService(typeof(TimerService))
                    ?? new TimerService();

                var vm = new SettingsViewModel(settingsService, hotkey, focusService, timerService);
                var window = new SettingsWindow
                {
                    DataContext = vm,
                    Owner = Application.Current?.MainWindow
                };

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing settings window: {ex.Message}");
            }
        }
    }
}
