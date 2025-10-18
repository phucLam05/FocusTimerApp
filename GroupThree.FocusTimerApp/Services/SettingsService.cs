using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.Services
{
    public class SettingsService
    {
        private readonly string _configPath;
        private ConfigSetting? _cachedSettings;

        public SettingsService(string? configPath = null)
        {
            _configPath = configPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        }

        // ==============================
        // Đọc toàn bộ config
        // ==============================
        public ConfigSetting LoadSettings()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    Console.WriteLine($"Config file not found. Creating new at {_configPath}");
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

        // ==============================
        // Ghi toàn bộ config
        // ==============================
        public void SaveSettings(ConfigSetting settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_configPath, json);
                _cachedSettings = settings;
                Console.WriteLine("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        // ==============================
        // Lấy danh sách Hotkeys nhanh
        // ==============================
        public List<HotkeyBinding> LoadHotkeys()
        {
            if (_cachedSettings == null)
                _cachedSettings = LoadSettings();

            return _cachedSettings.Hotkeys ?? new List<HotkeyBinding>();
        }

        // ==============================
        // Cập nhật Hotkeys và lưu
        // ==============================
        public void UpdateHotkeys(List<HotkeyBinding> newHotkeys)
        {
            if (_cachedSettings == null)
                _cachedSettings = LoadSettings();

            _cachedSettings.Hotkeys = newHotkeys;
            SaveSettings(_cachedSettings);
        }

        // ==============================
        // Reset cấu hình về mặc định
        // ==============================
        public void ResetToDefault()
        {
            var defaultSetting = new ConfigSetting();
            SaveSettings(defaultSetting);
            _cachedSettings = defaultSetting;
            Console.WriteLine("Settings reset to default.");
        }
    }
}
