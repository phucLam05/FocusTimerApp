using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.ViewModels
{
    /// <summary>
    /// Main ViewModel for the Settings Window
    /// Manages navigation between different settings sections
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly HotkeyService? _hotkeyService;
        private readonly IThemeService _themeService;
        private readonly TimerService? _timerService;
        private readonly INotificationService? _notificationService;

        private object _currentView = null!;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                if (SetProperty(ref _currentView, value))
                {
                    CurrentViewType = _currentView?.GetType().Name ?? string.Empty;
                }
            }
        }

        private string _currentViewType = string.Empty;
        public string CurrentViewType
        {
            get => _currentViewType;
            set => SetProperty(ref _currentViewType, value);
        }

        public ICommand ShowGeneralCommand { get; }
        public ICommand ShowNotificationCommand { get; }
        public ICommand ShowTimerCommand { get; }
        public ICommand ShowHotkeyCommand { get; }
        public ICommand ReloadHotkeysCommand { get; }

        /// <summary>
        /// Constructor without NotificationService (for backward compatibility)
        /// </summary>
        public SettingsViewModel(
            SettingsService settingsService,
            HotkeyService? hotkeyService,
            IThemeService themeService,
            TimerService? timerService)
            : this(settingsService, hotkeyService, themeService, timerService, null)
        {
        }

        /// <summary>
        /// Main constructor with full dependency injection
        /// </summary>
        public SettingsViewModel(
            SettingsService settingsService,
            HotkeyService? hotkeyService,
            IThemeService themeService,
            TimerService? timerService,
            INotificationService? notificationService)
        {
            _settingsService = settingsService;
            _hotkeyService = hotkeyService;
            _themeService = themeService;
            _timerService = timerService;
            _notificationService = notificationService;

            // Initialize navigation commands
            ShowGeneralCommand = new RelayCommand<object>(_ => ShowGeneral());
            ShowNotificationCommand = new RelayCommand<object>(_ => ShowNotification());
            ShowTimerCommand = new RelayCommand<object>(_ => ShowTimer());
            ShowHotkeyCommand = new RelayCommand<object>(_ => ShowHotkey());
            ReloadHotkeysCommand = new RelayCommand<object>(_ => ReloadHotkeys());

            // Show general settings by default
            ShowGeneral();
        }

        /// <summary>
        /// Shows the General Settings section
        /// </summary>
        private void ShowGeneral()
        {
            var vm = new GeneralSettingsViewModel(_settingsService, _themeService);
            CurrentView = vm; // App.xaml DataTemplate automatically maps VM to View
        }

        /// <summary>
        /// Shows the Notification Settings section with optional NotificationService for testing
        /// </summary>
        private void ShowNotification()
        {
            var vm = new NotificationSettingsViewModel(_settingsService, _notificationService);
            CurrentView = vm;
        }

        /// <summary>
        /// Shows the Timer Settings section with TimerService for real-time sync
        /// </summary>
        private void ShowTimer()
        {
            var vm = new TimerSettingsViewModel(_settingsService, _timerService);
            CurrentView = vm;
        }

        /// <summary>
        /// Shows the Hotkey Settings section
        /// </summary>
        private void ShowHotkey()
        {
            var vm = new HotkeySettingsViewModel(_settingsService, _hotkeyService);
            CurrentView = vm;
        }

        /// <summary>
        /// Reloads all hotkeys from settings (useful after settings change)
        /// </summary>
        private void ReloadHotkeys()
        {
            _hotkeyService?.ReloadHotkeys();
            System.Diagnostics.Debug.WriteLine("[SettingsViewModel] Hotkeys reloaded");
        }
    }
}
