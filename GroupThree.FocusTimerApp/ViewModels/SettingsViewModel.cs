using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsService _settingsService;
        private readonly HotkeyService? _hotkeyService;

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

        // Only DI constructor
        public SettingsViewModel(SettingsService settingsService, HotkeyService? hotkeyService)
        {
            _settingsService = settingsService;
            _hotkeyService = hotkeyService;

            ShowGeneralCommand = new RelayCommand<object>(_ => ShowGeneral());
            ShowNotificationCommand = new RelayCommand<object>(_ => ShowNotification());
            ShowTimerCommand = new RelayCommand<object>(_ => ShowTimer());
            ShowHotkeyCommand = new RelayCommand<object>(_ => ShowHotkey());
            ReloadHotkeysCommand = new RelayCommand<object>(_ => ReloadHotkeys());

            // default view
            ShowGeneral();
        }

        private void ShowGeneral()
        {
            var vm = new GeneralSettingsViewModel(_settingsService);
            CurrentView = vm; // set VM; App.xaml DataTemplate maps VM -> View
        }

        private void ShowNotification()
        {
            var vm = new NotificationSettingsViewModel(_settingsService);
            CurrentView = vm;
        }

        private void ShowTimer()
        {
            var vm = new TimerSettingsViewModel(_settingsService);
            CurrentView = vm;
        }

        private void ShowHotkey()
        {
            var vm = new HotkeySettingsViewModel(_settingsService, _hotkeyService);
            CurrentView = vm;
        }

        private void ReloadHotkeys()
        {
            _hotkeyService?.ReloadHotkeys();
        }
    }
}
