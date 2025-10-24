using System;
using System.Windows;
using GroupThree.FocusTimerApp.Views;
using GroupThree.FocusTimerApp.Services;
using Microsoft.Extensions.DependencyInjection;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp
{
    /// <summary>
    /// Main application class - handles startup, dependency injection setup, and global services
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        public static IServiceProvider? ServiceProvider { get; private set; }

        public HotkeyService? HotkeyServiceInstance { get; private set; }

        /// <summary>
        /// Application startup - configures DI, initializes services, and shows main window
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            ServiceProvider = _serviceProvider;

            // Resolve and show MainWindow from DI container
            var main = _serviceProvider.GetRequiredService<MainWindow>();
            main.Show();

            // Initialize HotkeyService after MainWindow is shown (needs window handle)
            InitializeHotkeyService(main);
        }

        /// <summary>
        /// Initializes and registers global hotkeys for the application
        /// </summary>
        private void InitializeHotkeyService(MainWindow main)
        {
            try
            {
                var settings = _serviceProvider!.GetRequiredService<SettingsService>();
                var mainVm = main.DataContext as MainViewModel;
                
                if (mainVm != null)
                {
                    // Create HotkeyService with window handle
                    var hk = new HotkeyService(main, settings);
                    
                    // Connect hotkey events to MainViewModel
                    hk.HotkeyPressed += action => mainVm.HandleHotkeyAction(action);
                    
                    // Register all hotkeys from settings
                    hk.RegisterHotkeys();

                    HotkeyServiceInstance = hk;
                    Console.WriteLine("[App] HotkeyService initialized successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[App] Hotkey initialization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Configures all services and dependencies for the application
        /// </summary>
        private void ConfigureServices(ServiceCollection services)
        {
            // Core services - Singletons (one instance throughout app lifetime)
            services.AddSingleton<SettingsService>();
            services.AddSingleton<TimerService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<INotificationService, NotificationService>();

            // WindowService needs service provider to resolve windows
            services.AddSingleton<WindowService>(sp => new WindowService(sp));

            // OverlayService factory pattern - creates new instances as needed
            services.AddTransient<OverlayViewModel>();
            services.AddTransient<IOverlayService>(sp => new OverlayService(sp));

            // ViewModels - Transient (new instance each time)
            services.AddTransient<MainViewModel>(sp =>
            {
                var timer = sp.GetRequiredService<TimerService>();
                var windowSvc = sp.GetRequiredService<WindowService>();
                var overlay = sp.GetRequiredService<IOverlayService>();
                var theme = sp.GetRequiredService<IThemeService>();
                var notification = sp.GetRequiredService<INotificationService>();
                return new MainViewModel(timer, windowSvc, overlay, theme, notification);
            });

            // Windows - Transient (new instance for each request)
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
                var theme = sp.GetRequiredService<IThemeService>();
                var timer = sp.GetRequiredService<TimerService>();
                var notification = sp.GetRequiredService<INotificationService>();
                
                // ✅ Get the ACTUAL HotkeyService instance that was initialized at startup
                var hotkey = (Application.Current as App)?.HotkeyServiceInstance;
                
                var vm = new SettingsViewModel(settings, hotkey, theme, timer, notification);
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
    }
}
