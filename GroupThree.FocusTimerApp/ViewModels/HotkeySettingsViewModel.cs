using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class HotkeySettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "Hotkey";

        private readonly SettingsService _settingsService;
        private readonly HotkeyService? _hotkeyService;

        public ObservableCollection<HotkeyBinding> Hotkeys { get; } = new();

        public ICommand ApplyCommand { get; }
        public ICommand ResetDefaultsCommand { get; }

        private static readonly string[] StandardActions = new[] { "Start", "Pause", "Stop", "ToggleOverlay" };

        public HotkeySettingsViewModel(SettingsService settingsService)
            : this(settingsService, null)
        {
        }

        public HotkeySettingsViewModel(SettingsService settingsService, HotkeyService? hotkeyService)
        {
            _settingsService = settingsService;
            _hotkeyService = hotkeyService;

            LoadFromConfig();

            ApplyCommand = new RelayCommand<object>(_ => Apply());
            ResetDefaultsCommand = new RelayCommand<object>(_ => ResetDefaults());
        }

        private void LoadFromConfig()
        {
            Hotkeys.Clear();
            var cfg = SettingsServiceBuilder();

            if (cfg.Hotkeys != null && cfg.Hotkeys.Count > 0)
            {
                foreach (var hk in cfg.Hotkeys)
                    Hotkeys.Add(hk);
            }

            foreach (var action in StandardActions)
            {
                if (!Hotkeys.Any(h => string.Equals(h.ActionName, action, System.StringComparison.OrdinalIgnoreCase)))
                {
                    Hotkeys.Add(new HotkeyBinding { ActionName = action, Key = string.Empty, Modifiers = string.Empty, Description = action });
                }
            }
        }

        private void Apply()
        {
            var cfg = SettingsServiceBuilder();
            cfg.Hotkeys = Hotkeys.ToList();
            _settingsService.SaveSettings(cfg);

            _hotkeyService?.ReloadHotkeys();

            ShowSuccessDialog("Settings Saved", "Hotkey settings have been saved successfully!");
        }

        private void ResetDefaults()
        {
            _settingsService.ResetToDefault();

            LoadFromConfig();

            _hotkeyService?.ReloadHotkeys();

            ShowSuccessDialog("Settings Reset", "Hotkey settings have been reset to default!");
        }

        private void ShowSuccessDialog(string title, string message)
        {
            try
            {
                var dialog = new Views.SuccessDialog(title, message)
                {
                    Owner = System.Windows.Application.Current?.MainWindow
                };
                dialog.ShowDialog();
            }
            catch { }
        }

        private Models.ConfigSetting SettingsServiceBuilder()
        {
            return _settingsService.LoadSettings();
        }
    }
}
