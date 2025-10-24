using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.ViewModels;
using GroupThree.FocusTimerApp.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Windows;

namespace GroupThree.FocusTimerApp
{
    public partial class App : System.Windows.Application
    {
        private IServiceProvider? _serviceProvider;
        public static IServiceProvider? ServiceProvider { get; private set; }

        public HotkeyService? HotkeyServiceInstance { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            ServiceProvider = _serviceProvider;

            // resolve MainWindow from DI
            var main = _serviceProvider!.GetRequiredService<MainWindow>();
            main.Show();

            // after MainWindow is shown, create HotkeyService and register in DI-backed property
            try
            {
                var settings = _serviceProvider!.GetRequiredService<SettingsService>();
                var mainVm = (main.DataContext as MainViewModel);
                if (mainVm != null)
                {
                    var hk = new HotkeyService(main, settings);
                    hk.HotkeyPressed += action => mainVm.HandleHotkeyAction(action);
                    hk.RegisterHotkeys();

                    HotkeyServiceInstance = hk;
                }
                var focusService = new AppFocusService();
                var timerService = _serviceProvider!.GetRequiredService<TimerService>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hotkey init error: {ex.Message}");
            }
        }

        //private void _service_provider_builder(ServiceCollection services)
        //{
        //    _serviceProvider = services.BuildServiceProvider();
        //    ServiceProvider = _serviceProvider;
        //}

        private void ConfigureServices(ServiceCollection services)
        {
            // services and singletons
            services.AddSingleton<SettingsService>();
            services.AddSingleton<TimerService>();

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
                return new MainViewModel(timer, windowSvc, overlay);
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
                var focusService = new AppFocusService();
                var timerService = sp.GetRequiredService<TimerService>(); // 🔥 thêm dòng này

                var settingsVM = new SettingsViewModel(settings, hotkey, focusService, timerService); // 🔥 truyền thêm timerService
                var win = new SettingsWindow();
                win.DataContext = settingsVM;
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
