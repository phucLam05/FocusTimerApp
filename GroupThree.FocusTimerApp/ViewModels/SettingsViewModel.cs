using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly HotkeyService? _hotkeyService;
        private readonly AppFocusService _focusService;
        private readonly TimerService _timerService;
        private readonly Services.ThemeService _themeService;

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
        public ICommand ShowFocusZoneCommand { get; }
        public ICommand ShowExportImportCommand { get; }

        public SettingsViewModel(SettingsService settingsService, Services.ThemeService themeService, HotkeyService? hotkeyService, AppFocusService focusService, TimerService timerService)
        {
            _settingsService = settingsService;
            _hotkeyService = hotkeyService;
            _focusService = focusService;
            _timerService = timerService;
            _themeService = themeService;

            ShowGeneralCommand = new RelayCommand<object>(_ => ShowGeneral());
            ShowNotificationCommand = new RelayCommand<object>(_ => ShowNotification());
            ShowTimerCommand = new RelayCommand<object>(_ => ShowTimer());
            ShowHotkeyCommand = new RelayCommand<object>(_ => ShowHotkey());
            ReloadHotkeysCommand = new RelayCommand<object>(_ => ReloadHotkeys());
            ShowFocusZoneCommand = new RelayCommand<object>(_ => ShowFocusZone());
            ShowExportImportCommand = new RelayCommand<object>(_ => ShowExportImport());

            // default view
            ShowGeneral();
        }

        private void ShowGeneral() =>
            CurrentView = new GeneralSettingsViewModel(_settingsService, _themeService);

        private void ShowNotification() =>
            CurrentView = new NotificationSettingsViewModel(_settingsService);

        private void ShowTimer() =>
            CurrentView = new TimerSettingsViewModel(_settingsService);

        private void ShowHotkey() =>
            CurrentView = new HotkeySettingsViewModel(_settingsService, _hotkeyService);

        private void ShowExportImport() =>
            CurrentView = new ExportImportSettingsViewModel(_settingsService, _hotkeyService);


        private void ReloadHotkeys() =>
            _hotkeyService?.ReloadHotkeys();

        private void ShowFocusZone() =>
            CurrentView = new AppControlViewModel(_focusService, _timerService);
    }
}
