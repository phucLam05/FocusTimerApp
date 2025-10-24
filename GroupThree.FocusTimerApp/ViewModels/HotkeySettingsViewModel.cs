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

        private ObservableCollection<HotkeyBinding> _hotkeys = new();
        
        /// <summary>
        /// Collection of all available hotkey bindings
        /// </summary>
        public ObservableCollection<HotkeyBinding> Hotkeys 
        { 
            get => _hotkeys;
            set
            {
                _hotkeys = value;
                RaisePropertyChanged(nameof(Hotkeys));
            }
        }

        public ICommand ApplyCommand { get; }
        public ICommand ResetDefaultsCommand { get; }
        public ICommand EditHotkeyCommand { get; }

        /// <summary>
        /// Standard actions that must always be present and cannot be removed
        /// </summary>
        private static readonly string[] StandardActions = new[] 
        { 
            "ToggleOverlay",
            "Start", 
            "Pause", 
            "Stop"
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

            // Initialize commands first
            ApplyCommand = new RelayCommand<object>(_ => Apply());
            ResetDefaultsCommand = new RelayCommand<object>(_ => ResetDefaults());
            EditHotkeyCommand = new RelayCommand<HotkeyBinding>(EditHotkey);
            
            // Load existing hotkey configuration
            LoadFromConfig();
        }

        /// <summary>
        /// Loads hotkey bindings from configuration
        /// Ensures all standard actions are present by merging with defaults
        /// </summary>
        private void LoadFromConfig()
        {
            System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Loading config from: {_settingsService.GetType().Name}");
            
            var cfg = _settingsService.LoadSettings();

            System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Config loaded, Hotkeys count: {cfg.Hotkeys?.Count ?? 0}");
            
            var newCollection = new ObservableCollection<HotkeyBinding>();
            
            // Load existing hotkeys from config
            if (cfg.Hotkeys != null && cfg.Hotkeys.Count > 0)
            {
                foreach (var hk in cfg.Hotkeys)
                {
                    System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Processing: {hk.ActionName} - Key: '{hk.Key}', Modifiers: '{hk.Modifiers}'");
                    
                    // Create new instance with explicit property setting
                    var newHotkey = new HotkeyBinding
                    {
                        ActionName = hk.ActionName,
                        Description = hk.Description
                    };
                    
                    // Set Key and Modifiers AFTER object creation to trigger UpdateDisplayText
                    newHotkey.Modifiers = hk.Modifiers ?? string.Empty;
                    newHotkey.Key = hk.Key ?? string.Empty;
                    
                    // Mark as registered if key is configured (assume it's working)
                    newHotkey.IsRegistered = !string.IsNullOrEmpty(hk.Key);
                    
                    System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Created: {newHotkey.ActionName} DisplayText='{newHotkey.DisplayText}'");
                    
                    newCollection.Add(newHotkey);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] WARNING: No hotkeys in config! Adding defaults...");
                
                // Add default hotkeys if config is empty
                newCollection.Add(CreateHotkey("ToggleOverlay", "Q", "Ctrl+Alt", "Show/Hide overlay"));
                newCollection.Add(CreateHotkey("Start", "W", "Ctrl+Alt", "Start timer"));
                newCollection.Add(CreateHotkey("Pause", "E", "Ctrl+Alt", "Pause timer"));
                newCollection.Add(CreateHotkey("Stop", "R", "Ctrl+Alt", "Stop timer"));
            }

            // Ensure all standard actions exist
            foreach (var action in StandardActions)
            {
                if (!newCollection.Any(h => string.Equals(h.ActionName, action, StringComparison.OrdinalIgnoreCase)))
                {
                    System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Adding missing standard action: {action}");
                    newCollection.Add(CreateHotkey(action, "", "", action));
                }
            }

            // Replace entire collection
            Hotkeys = newCollection;

            System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Total loaded: {Hotkeys.Count} hotkeys");
            
            // Debug: Print all
            foreach (var hk in Hotkeys)
            {
                System.Diagnostics.Debug.WriteLine($"  Final in collection: {hk.ActionName} DisplayText='{hk.DisplayText}' Key='{hk.Key}' Mod='{hk.Modifiers}'");
            }
        }
        
        private HotkeyBinding CreateHotkey(string actionName, string key, string modifiers, string description)
        {
            var hotkey = new HotkeyBinding
            {
                ActionName = actionName,
                Description = description
            };
            
            // Set in correct order to trigger UpdateDisplayText
            hotkey.Modifiers = modifiers ?? string.Empty;
            hotkey.Key = key ?? string.Empty;
            
            // Mark as registered if key is configured
            hotkey.IsRegistered = !string.IsNullOrEmpty(key);
            
            System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Created hotkey: {actionName} = '{hotkey.DisplayText}' (Key={key}, Mod={modifiers})");
            return hotkey;
        }

        /// <summary>
        /// Applies and saves the current hotkey configuration
        /// SIMPLIFIED: Just save to JSON and reload hotkeys
        /// </summary>
        private void Apply()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Saving {Hotkeys.Count} hotkeys to config...");
                
                // Check for duplicate hotkeys
                var conflicts = new Dictionary<string, List<string>>();
                
                foreach (var hotkey in Hotkeys)
                {
                    if (string.IsNullOrEmpty(hotkey.Key)) continue;
                    
                    string combo = $"{hotkey.Modifiers}+{hotkey.Key}";
                    
                    if (!conflicts.ContainsKey(combo))
                        conflicts[combo] = new List<string>();
                    
                    conflicts[combo].Add(hotkey.ActionName);
                }
                
                var duplicates = conflicts.Where(kvp => kvp.Value.Count > 1).ToList();
                
                if (duplicates.Count > 0)
                {
                    var errorMessages = duplicates.Select(kvp => 
                        $"  {kvp.Key} is used by: {string.Join(", ", kvp.Value)}");
                    
                    CustomMessageBox.Show(
                        $"Duplicate hotkeys detected!\n\n" +
                        $"Each action must have a unique key combination:\n\n" +
                        string.Join("\n", errorMessages) +
                        "\n\nPlease assign different keys to these actions.",
                        "Duplicate Hotkeys",
                        CustomMessageBox.MessageType.Warning
                    );
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine("[HotkeySettings] No duplicates, saving to config...");
                
                // Save to config
                var cfg = _settingsService.LoadSettings();
                cfg.Hotkeys = Hotkeys.ToList();
                _settingsService.SaveSettings(cfg);
                
                System.Diagnostics.Debug.WriteLine("[HotkeySettings] Config saved successfully!");
                
                // Reload hotkeys in the service (unregister old + register new)
                _hotkeyService?.ReloadHotkeys();
                
                // Mark all configured hotkeys as registered
                foreach (var hotkey in Hotkeys)
                {
                    if (!string.IsNullOrEmpty(hotkey.Key))
                    {
                        hotkey.IsRegistered = true;
                    }
                }
                
                // Show success message
                var totalConfigured = Hotkeys.Count(h => !string.IsNullOrEmpty(h.Key));
                
                CustomMessageBox.Show(
                    $"? Hotkey settings saved successfully!\n\n" +
                    $"{totalConfigured} hotkey(s) configured and active.\n\n" +
                    $"Your keyboard shortcuts are ready to use!",
                    "Settings Saved",
                    CustomMessageBox.MessageType.Success
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Save error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to save hotkey settings:\n\n{ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }

        /// <summary>
        /// Resets ONLY hotkey settings to default configuration without affecting other settings
        /// </summary>
        private void ResetDefaults()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[HotkeySettings] Resetting hotkey settings to defaults...");
                
                // Create NEW collection with default hotkeys using CreateHotkey
                var newCollection = new ObservableCollection<HotkeyBinding>
                {
                    CreateHotkey("ToggleOverlay", "Q", "Ctrl+Alt", "Show/Hide overlay"),
                    CreateHotkey("Start", "W", "Ctrl+Alt", "Start timer"),
                    CreateHotkey("Pause", "E", "Ctrl+Alt", "Pause timer"),
                    CreateHotkey("Stop", "R", "Ctrl+Alt", "Stop timer")
                };
                
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Created new collection with {newCollection.Count} hotkeys");
                
                // Replace entire collection to force UI update
                Hotkeys = newCollection;
                
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Assigned to Hotkeys property, count={Hotkeys.Count}");
                
                // Load current config
                var cfg = _settingsService.LoadSettings();
                
                // Update config with defaults
                cfg.Hotkeys = Hotkeys.ToList();
                
                // Save config (keeps other settings intact)
                _settingsService.SaveSettings(cfg);

                // Reload hotkeys in the service
                _hotkeyService?.ReloadHotkeys();
                
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Reset complete");
                
                // Show info message
                CustomMessageBox.Show(
                    "Hotkey settings have been reset to defaults!\n\n" +
                    "ToggleOverlay: Ctrl+Alt+Q\n" +
                    "Start: Ctrl+Alt+W\n" +
                    "Pause: Ctrl+Alt+E\n" +
                    "Stop: Ctrl+Alt+R",
                    "Reset Complete",
                    CustomMessageBox.MessageType.Info
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Reset error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Stack trace: {ex.StackTrace}");
                CustomMessageBox.Show(
                    $"Failed to reset hotkey settings: {ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }

        /// <summary>
        /// Opens dialog to edit a specific hotkey binding
        /// </summary>
        private void EditHotkey(HotkeyBinding? hotkey)
        {
            if (hotkey == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Editing hotkey: {hotkey.ActionName}");
                
                // Temporarily disable all hotkeys while editing to allow key capture
                System.Diagnostics.Debug.WriteLine("[HotkeySettings] Temporarily unregistering all hotkeys...");
                _hotkeyService?.UnregisterAll();
                
                // Open HotkeyInputBox dialog
                var inputBox = new Views.HotkeyInputBox(
                    hotkey.ActionName,
                    hotkey.Key,
                    hotkey.Modifiers
                );
                
                // Set owner to center on parent window
                inputBox.Owner = System.Windows.Application.Current.MainWindow;
                
                bool? dialogResult = inputBox.ShowDialog();
                
                // Re-register hotkeys after dialog closes
                System.Diagnostics.Debug.WriteLine("[HotkeySettings] Re-registering all hotkeys...");
                _hotkeyService?.RegisterHotkeys();
                
                if (dialogResult == true)
                {
                    System.Diagnostics.Debug.WriteLine($"[HotkeySettings] User set new hotkey: Key={inputBox.ResultKey}, Mod={inputBox.ResultModifiers}");
                    
                    // Check for duplicate hotkeys (only if user set a key)
                    if (!string.IsNullOrEmpty(inputBox.ResultKey))
                    {
                        var duplicate = Hotkeys.FirstOrDefault(h => 
                            h != hotkey && 
                            h.Key == inputBox.ResultKey && 
                            h.Modifiers == inputBox.ResultModifiers);
                        
                        if (duplicate != null)
                        {
                            CustomMessageBox.Show(
                                $"This hotkey combination is already used by '{duplicate.ActionName}'.\n\n" +
                                $"Please choose a different combination.",
                                "Duplicate Hotkey",
                                CustomMessageBox.MessageType.Warning
                            );
                            return;
                        }
                    }
                    
                    // Update hotkey
                    hotkey.Modifiers = inputBox.ResultModifiers;
                    hotkey.Key = inputBox.ResultKey;
                    
                    System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Updated hotkey: {hotkey.ActionName} = {hotkey.DisplayText}");
                    
                    // Note: Changes are not saved until user clicks "Apply Changes"
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HotkeySettings] Edit error: {ex.Message}");
                
                // Make sure to re-register even if error occurs
                try
                {
                    _hotkeyService?.RegisterHotkeys();
                }
                catch { }
                
                CustomMessageBox.Show(
                    $"Failed to edit hotkey:\n\n{ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }
    }
}
