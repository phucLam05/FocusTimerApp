using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Views;

namespace GroupThree.FocusTimerApp.ViewModels
{
    /// <summary>
    /// ViewModel for Hotkey Settings page
    /// Manages keyboard shortcut configuration and registration
    /// </summary>
    public class HotkeySettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "Hotkey";

        private readonly SettingsService _settingsService;
        private readonly HotkeyService? _hotkeyService;

        /// <summary>
        /// Collection of all available hotkey bindings
        /// </summary>
        public ObservableCollection<HotkeyBinding> Hotkeys { get; } = new();

        public ICommand ApplyCommand { get; }
        public ICommand ResetDefaultsCommand { get; }

        /// <summary>
        /// Standard actions that must always be present and cannot be removed
        /// </summary>
        private static readonly string[] StandardActions = new[] 
        { 
            "Start", 
            "Pause", 
            "Stop", 
            "ToggleOverlay" 
        };

        /// <summary>
        /// Constructor for simple DI (without HotkeyService)
        /// </summary>
        public HotkeySettingsViewModel(SettingsService settingsService)
            : this(settingsService, null)
        {
        }

        /// <summary>
        /// Main constructor with full dependency injection
        /// </summary>
        public HotkeySettingsViewModel(SettingsService settingsService, HotkeyService? hotkeyService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _hotkeyService = hotkeyService;

            // Load existing hotkey configuration
            LoadFromConfig();

            // Initialize commands
            ApplyCommand = new RelayCommand<object>(_ => Apply());
            ResetDefaultsCommand = new RelayCommand<object>(_ => ResetDefaults());
        }

        /// <summary>
        /// Loads hotkey bindings from configuration
        /// Ensures all standard actions are present by merging with defaults
        /// </summary>
        private void LoadFromConfig()
        {
            Hotkeys.Clear();
            var cfg = _settingsService.LoadSettings();

            // Load existing hotkeys from config
            if (cfg.Hotkeys != null && cfg.Hotkeys.Count > 0)
            {
                foreach (var hk in cfg.Hotkeys)
                {
                    Hotkeys.Add(hk);
                }
            }

            // Ensure all standard actions exist (merge defaults if missing)
            foreach (var action in StandardActions)
            {
                if (!Hotkeys.Any(h => string.Equals(h.ActionName, action, StringComparison.OrdinalIgnoreCase)))
                {
                    Hotkeys.Add(new HotkeyBinding 
                    { 
                        ActionName = action, 
                        Key = string.Empty, 
                        Modifiers = string.Empty, 
                        Description = action 
                    });
                }
            }

            System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Loaded {Hotkeys.Count} hotkey bindings");
        }

        /// <summary>
        /// Applies and saves the current hotkey configuration
        /// Reloads hotkeys in the HotkeyService to register changes
        /// </summary>
        private void Apply()
        {
            try
            {
                // Save configuration
                var cfg = _settingsService.LoadSettings();
                cfg.Hotkeys = Hotkeys.ToList();
                _settingsService.SaveSettings(cfg);

                // Reload hotkeys in the service to register changes
                _hotkeyService?.ReloadHotkeys();
                
                System.Diagnostics.Debug.WriteLine("[HotkeySettings] Settings applied and hotkeys reloaded");
                
                // Show success message
                CustomMessageBox.Show(
                    "Your hotkey settings have been applied successfully!",
                    "Settings Applied",
                    CustomMessageBox.MessageType.Success
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Apply error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to apply hotkey settings: {ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }

        /// <summary>
        /// Resets all hotkeys to default configuration
        /// </summary>
        private void ResetDefaults()
        {
            try
            {
                // Reset to default settings
                _settingsService.ResetToDefault();

                // Reload the hotkey collection from default config
                LoadFromConfig();

                // Reload hotkeys in the service
                _hotkeyService?.ReloadHotkeys();
                
                System.Diagnostics.Debug.WriteLine("[HotkeySettings] Reset to defaults completed");
                
                // Show info message
                CustomMessageBox.Show(
                    "Hotkey settings have been reset to defaults!",
                    "Reset Complete",
                    CustomMessageBox.MessageType.Info
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Reset error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to reset hotkey settings: {ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }
    }
}
