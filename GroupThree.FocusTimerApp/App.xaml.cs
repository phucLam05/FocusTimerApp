using System;
using System.Windows;
using GroupThree.FocusTimerApp.Views;
using GroupThree.FocusTimerApp.Services;
using Microsoft.Extensions.DependencyInjection;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp
{
    public partial class App : System.Windows.Application
    {
        private IServiceProvider? _serviceProvider;
        public static IServiceProvider? ServiceProvider { get; private set; }

        public HotkeyService? HotkeyServiceInstance { get; private set; }
        public static bool IsExiting { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _service_provider_builder(services);

            var settingsService = _serviceProvider!.GetRequiredService<SettingsService>();
            var startupService = _serviceProvider!.GetRequiredService<StartupService>();
            var cfg = settingsService.LoadSettings();

            // Apply Start with Windows
            startupService.ApplyStartupSetting(cfg.General?.StartWithWindows ?? false);

            // resolve MainWindow from DI and show
            var main = _serviceProvider!.GetRequiredService<MainWindow>();
            main.Show();

            // after MainWindow stage, init HotkeyService
            try
            {
                var mainVm = (main.DataContext as MainViewModel);
                if (mainVm != null)
                {
                    var hk = new HotkeyService(main, settingsService);
                    hk.HotkeyPressed += action => mainVm.HandleHotkeyAction(action);
                    hk.RegisterHotkeys();
                    HotkeyServiceInstance = hk;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hotkey init error: {ex.Message}");
            }
        }

        private void _service_provider_builder(ServiceCollection services)
        {
            _serviceProvider = services.BuildServiceProvider();
            ServiceProvider = _serviceProvider;
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // services and singletons
            services.AddSingleton<SettingsService>();
            services.AddSingleton<StartupService>();
            services.AddSingleton<TimerService>(sp =>
            {
                var timer = new TimerService();
                var settings = sp.GetRequiredService<SettingsService>();
                ApplySettingsToTimer(timer, settings.LoadSettings());
                settings.SettingsChanged += cfg => ApplySettingsToTimer(timer, cfg);
                return timer;
            });

            // WindowService needs service provider
            services.AddSingleton<WindowService>(sp => new WindowService(sp));

            // Register OverlayViewModel and OverlayService factory
            services.AddTransient<OverlayViewModel>();
            services.AddTransient<IOverlayService>(sp => new OverlayService(sp));

            // ViewModels
            services.AddTransient<MainViewModel>(sp =>
            {
                var timer = sp.GetRequiredService<TimerService>();
                var windowSvc = sp.GetRequiredService<WindowService>();
                var overlay = sp.GetRequiredService<IOverlayService>();
                var settings = sp.GetRequiredService<SettingsService>();
                return new MainViewModel(timer, windowSvc, overlay, settings);
            });

            // windows
            services.AddTransient<MainWindow>(sp =>
            {
                var vm = sp.GetRequiredService<MainViewModel>();
                var settings = sp.GetRequiredService<SettingsService>();
                var timer = sp.GetRequiredService<TimerService>();
                var windowSvc = sp.GetRequiredService<WindowService>();
                var overlay = sp.GetRequiredService<IOverlayService>();
                var win = new MainWindow(vm, settings, timer, windowSvc, overlay);
                win.DataContext = vm;
                return win;
            });

            services.AddTransient<SettingsWindow>(sp =>
            {
                var settings = sp.GetRequiredService<SettingsService>();
                var hotkey = (System.Windows.Application.Current as App)?.HotkeyServiceInstance;
                var vm = new SettingsViewModel(settings, hotkey);
                var win = new SettingsWindow();
                win.DataContext = vm;
                return win;
            });

            services.AddTransient<OverlayWindow>(sp =>
            {
                var vm = sp.GetRequiredService<OverlayViewModel>();
                var win = new OverlayWindow();
                win.DataContext = vm;
                return win;
            });
        }

        private static void ApplySettingsToTimer(TimerService timer, GroupThree.FocusTimerApp.Models.ConfigSetting cfg)
        {
            if (cfg == null) return;
            try
            {
                // Map minutes from settings to timer engine
                var t = cfg.TimerSettings;
                if (t != null)
                {
                    timer.WorkDuration = TimeSpan.FromMinutes(Math.Max(0, t.WorkDuration));
                    timer.ShortBreak = TimeSpan.FromMinutes(Math.Max(0, t.BreakDuration));
                    timer.LongBreak = TimeSpan.FromMinutes(Math.Max(0, t.LongBreakDuration));
                    timer.ShortBreakAfter = TimeSpan.FromMinutes(Math.Max(0, t.WorkDuration)); // not used directly in new flow
                    timer.LongBreakAfterShortBreakCount = Math.Max(1, t.LongBreakEvery);
                    timer.ReminderInterval = TimeSpan.FromMinutes(Math.Max(0, t.TrackingInterval));
                }

                // Notification master switch
                timer.NotificationsEnabled = cfg.Notification?.EnableNotifications ?? true;

                // Apply StartWithWindows on change
                try
                {
                    var startup = ServiceProvider?.GetService(typeof(StartupService)) as StartupService;
                    startup?.ApplyStartupSetting(cfg.General?.StartWithWindows ?? false);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ApplySettingsToTimer error: {ex.Message}");
            }
        }
    }
}
