using System.Windows;
using GroupThree.FocusTimerApp.ViewModels;
using GroupThree.FocusTimerApp.Services;
using System.ComponentModel;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class MainWindow : Window
    {
        public SettingsService SettingsService { get; }
        public HotkeyService? HotkeyService { get; private set; }
        public TimerService TimerService { get; }
        public WindowService WindowService { get; }
        private readonly IOverlayService _overlayService;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
        }

        public MainWindow(MainViewModel vm, SettingsService settingsService, TimerService timerService, WindowService windowService, IOverlayService overlayService)
        {
            InitializeComponent();

            SettingsService = settingsService;
            TimerService = timerService;
            WindowService = windowService;
            _overlayService = overlayService;

            this.DataContext = vm;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                var cfg = SettingsService?.LoadSettings();
                bool runInBackground = cfg?.General?.RunInBackground ?? false;
                if (runInBackground)
                {
                    // Keep app running in tray
                    e.Cancel = true;
                    this.ShowInTaskbar = false;
                    this.Hide();
                }
                else
                {
                    // Exit app normally
                    e.Cancel = false;
                }
            }
            catch
            {
                // if any error, allow close
                e.Cancel = false;
            }
        }
    }
}
