using System.Windows;
using GroupThree.FocusTimerApp.ViewModels;
using GroupThree.FocusTimerApp.Services;

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
        }

        public MainWindow(MainViewModel vm, SettingsService settingsService, TimerService timerService, WindowService windowService, IOverlayService overlayService)
        {
            InitializeComponent();

            SettingsService = settingsService;
            TimerService = timerService;
            WindowService = windowService;
            _overlayService = overlayService;

            this.DataContext = vm;
        }
    }
}
