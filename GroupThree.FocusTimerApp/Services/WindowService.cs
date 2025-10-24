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

                // Try to resolve SettingsWindow via DI
                var window = (SettingsWindow?)_serviceProvider.GetService(typeof(SettingsWindow));
                if (window == null)
                {
                    // create settings VM and window manually
                    var settingsService = (SettingsService?)_serviceProvider.GetService(typeof(SettingsService)) ?? new SettingsService();
                    var vm = new SettingsViewModel(settingsService, hotkey);
                    window = new SettingsWindow();
                    window.DataContext = vm;
                }

                window.Owner = Application.Current?.MainWindow;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing settings window: {ex.Message}");
            }
        }
    }
}
