﻿using System;
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

        public event Action<ConfigSetting>? SettingsChanged;

        public SettingsService(string? configPath = null)
        {
            // Default location: %AppData%\FocusTimerApp\AppSettings.json
            // If legacy config exists in application base folder, migrate it on first run.
            var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var targetDir = Path.Combine(appDataDir, "FocusTimerApp");
            var targetPath = Path.Combine(targetDir, "AppSettings.json");

            if (configPath == null)
            {
                try
                {
                    Directory.CreateDirectory(targetDir);

                    // Legacy locations
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var legacyJsonPath = Path.Combine(baseDir, "json", "AppSettings.json");
                    var legacyFlatPath = Path.Combine(baseDir, "appsettings.json");

                    // If new file doesn't exist yet, migrate from legacy if available
                    if (!File.Exists(targetPath))
                    {
                        if (File.Exists(legacyJsonPath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                            File.Copy(legacyJsonPath, targetPath, overwrite: true);
                        }
                        else if (File.Exists(legacyFlatPath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                            File.Copy(legacyFlatPath, targetPath, overwrite: true);
                        }
                    }
                }
                catch
                {
                    // ignore migration failures; file will be created on first save
                }
            }

            _configPath = configPath ?? targetPath;
        }

        public ConfigSetting LoadSettings()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    var defaultSetting = new ConfigSetting();
                    SaveSettings(defaultSetting);
                    return defaultSetting;
                }

                string json = File.ReadAllText(_configPath);
                _cachedSettings = JsonSerializer.Deserialize<ConfigSetting>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new ConfigSetting();

                return _cachedSettings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return new ConfigSetting();
            }
        }

        public void SaveSettings(ConfigSetting settings)
        {
            try
            {
                // ensure directory exists
                var dir = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_configPath, json);
                _cachedSettings = settings;
                SettingsChanged?.Invoke(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public List<HotkeyBinding> LoadHotkeys()
        {
            if (_cachedSettings == null)
                _cachedSettings = LoadSettings();

            return _cachedSettings.Hotkeys ?? new List<HotkeyBinding>();
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
            var defaultSetting = new ConfigSetting();
            SaveSettings(defaultSetting);
            _cachedSettings = defaultSetting;
            Console.WriteLine("Settings reset to default.");
        }
    }
}
