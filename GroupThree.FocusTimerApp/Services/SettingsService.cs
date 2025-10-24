using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _configPath;
        private ConfigSetting? _cachedSettings;

        public SettingsService(string? configPath = null)
        {
            _configPath = configPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            System.Diagnostics.Debug.WriteLine($"[SettingsService] Config path: {_configPath}");
        }

        public ConfigSetting LoadSettings()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Loading settings from: {_configPath}");
                System.Diagnostics.Debug.WriteLine($"[SettingsService] File exists: {File.Exists(_configPath)}");
                
                if (!File.Exists(_configPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[SettingsService] Config file not found, creating default");
                    var defaultSetting = new ConfigSetting();
                    SaveSettings(defaultSetting);
                    return defaultSetting;
                }

                string json = File.ReadAllText(_configPath);
                System.Diagnostics.Debug.WriteLine($"[SettingsService] JSON content length: {json.Length}");
                
                _cachedSettings = JsonSerializer.Deserialize<ConfigSetting>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new ConfigSetting();

                System.Diagnostics.Debug.WriteLine($"[SettingsService] Loaded config with {_cachedSettings.Hotkeys?.Count ?? 0} hotkeys");
                
                if (_cachedSettings.Hotkeys != null)
                {
                    foreach (var hk in _cachedSettings.Hotkeys)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {hk.ActionName}: Key={hk.Key}, Mod={hk.Modifiers}");
                    }
                }

                return _cachedSettings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Error loading settings: {ex.Message}");
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return new ConfigSetting();
            }
        }

        public void SaveSettings(ConfigSetting settings)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Saving {settings.Hotkeys?.Count ?? 0} hotkeys to: {_configPath}");
                
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_configPath, json);
                _cachedSettings = settings;
                
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Settings saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsService] Error saving settings: {ex.Message}");
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public List<HotkeyBinding> LoadHotkeys()
        {
            // Always reload to get fresh data
            _cachedSettings = null;
            var settings = LoadSettings();
            
            System.Diagnostics.Debug.WriteLine($"[SettingsService] LoadHotkeys returning {settings.Hotkeys?.Count ?? 0} items");
            
            return settings.Hotkeys ?? new List<HotkeyBinding>();
        }

        public void UpdateHotkeys(List<HotkeyBinding> newHotkeys)
        {
            if (_cachedSettings == null)
                _cachedSettings = LoadSettings();

            _cachedSettings.Hotkeys = newHotkeys;
            SaveSettings(_cachedSettings);
        }

        public void ResetToDefault()
        {
            System.Diagnostics.Debug.WriteLine("[SettingsService] Resetting to default configuration");
            
            var defaultSetting = new ConfigSetting();
            SaveSettings(defaultSetting);
            _cachedSettings = defaultSetting;
            
            System.Diagnostics.Debug.WriteLine($"[SettingsService] Reset complete, default has {defaultSetting.Hotkeys?.Count ?? 0} hotkeys");
            Console.WriteLine("Settings reset to default.");
        }
    }
}
