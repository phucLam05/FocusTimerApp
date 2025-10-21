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

        // Standard actions we want to show and cannot remove
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
            var cfg = _settings_service_builder();

            if (cfg.Hotkeys != null && cfg.Hotkeys.Count > 0)
            {
                foreach (var hk in cfg.Hotkeys)
                    Hotkeys.Add(hk);
            }

            // Ensure standard action rows exist (merge defaults)
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
            // Save into config and reload hotkeys
            var cfg = _settings_service_builder();
            cfg.Hotkeys = Hotkeys.ToList();
            _settingsService.SaveSettings(cfg);

            _hotkeyService?.ReloadHotkeys();
        }

        private void ResetDefaults()
        {
            _settingsService.ResetToDefault();

            // reload collection from default config
            LoadFromConfig();

            _hotkeyService?.ReloadHotkeys();
        }

        private HotkeyService? _hotkey_service_builder()
        {
            return _hotkeyService;
        }

        private Models.ConfigSetting _settings_service_builder()
        {
            return _settingsService.LoadSettings();
        }
    }
}
