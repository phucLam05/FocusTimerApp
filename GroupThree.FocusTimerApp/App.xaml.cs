using System;
using System.Windows;
using GroupThree.FocusTimerApp.Views;
using GroupThree.FocusTimerApp.Services;
using Microsoft.Extensions.DependencyInjection;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        public static IServiceProvider? ServiceProvider { get; private set; }

        public HotkeyService? HotkeyServiceInstance { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _service_provider_builder(services);

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
                var hotkey = (Application.Current as App)?.HotkeyServiceInstance;
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
    }
}
