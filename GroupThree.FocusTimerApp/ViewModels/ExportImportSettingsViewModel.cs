using System;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;
using System.IO;
using System.IO.Compression;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class ExportImportSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "Configuration Code";

        private readonly SettingsService _settingsService;
        private readonly HotkeyService? _hotkeyService;

        private string _exportCode = string.Empty;
        public string ExportCode { get => _exportCode; set => SetProperty(ref _exportCode, value); }

        private string _importCode = string.Empty;
        public string ImportCode { get => _importCode; set => SetProperty(ref _importCode, value); }

        private string _statusMessage = string.Empty;
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand CopyExportCodeCommand { get; }
        public ICommand GenerateExportCodeCommand { get; }
        public ICommand ImportFromCodeCommand { get; }

        public ExportImportSettingsViewModel(SettingsService settingsService, HotkeyService? hotkeyService)
        {
            _settingsService = settingsService;
            _hotkeyService = hotkeyService;

            CopyExportCodeCommand = new RelayCommand<object>(_ => CopyExportCode());
            GenerateExportCodeCommand = new RelayCommand<object>(_ => GenerateExportCode());
            ImportFromCodeCommand = new RelayCommand<object>(_ => ImportFromCode());

            GenerateExportCode();
        }

        private void CopyExportCode()
        {
            try
            {
                System.Windows.Clipboard.SetText(ExportCode ?? string.Empty);
                StatusMessage = "Configuration code copied.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cannot copy: {ex.Message}";
            }
        }

        private static string Base64UrlEncode(byte[] data)
        {
            // URL-safe Base64 without padding
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            // Remove whitespace/newlines accidentally pasted
            string s = (input ?? string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace(" ", string.Empty)
                .Trim();

            // Convert URL-safe alphabet back
            s = s.Replace('-', '+').Replace('_', '/');

            // Restore padding
            int mod4 = s.Length % 4;
            if (mod4 > 0)
            {
                s = s.PadRight(s.Length + (4 - mod4), '=');
            }

            return Convert.FromBase64String(s);
        }

        private static byte[] CompressBrotli(byte[] input)
        {
            using var ms = new MemoryStream();
            using (var brotli = new BrotliStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            {
                brotli.Write(input, 0, input.Length);
            }
            return ms.ToArray();
        }

        private static byte[] DecompressBrotli(byte[] input)
        {
            using var inputMs = new MemoryStream(input);
            using var brotli = new BrotliStream(inputMs, CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            brotli.CopyTo(outMs);
            return outMs.ToArray();
        }

        private void GenerateExportCode()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = false });
                var bytes = Encoding.UTF8.GetBytes(json);

                // Compress to make the code much shorter
                var compressed = CompressBrotli(bytes);
                var b64Url = Base64UrlEncode(compressed);

                ExportCode = b64Url;
                StatusMessage = "Configuration code generated from current settings.";
            }
            catch (Exception ex)
            {
                ExportCode = string.Empty;
                StatusMessage = $"Failed to generate code: {ex.Message}";
            }
        }

        private void ImportFromCode()
        {
            if (string.IsNullOrWhiteSpace(ImportCode))
            {
                StatusMessage = "Import code is empty.";
                return;
            }

            try
            {
                // Decode URL-safe Base64
                var raw = Base64UrlDecode(ImportCode);

                // Only support short compressed format
                byte[] jsonBytes;
                try
                {
                    jsonBytes = DecompressBrotli(raw);
                }
                catch (InvalidDataException)
                {
                    StatusMessage = "Invalid code (Brotli decompression failed).";
                    return;
                }
                catch (IOException ex)
                {
                    StatusMessage = $"Invalid code (Brotli I/O error): {ex.Message}";
                    return;
                }

                var json = Encoding.UTF8.GetString(jsonBytes);

                // Deserialize and basic validation
                var cfg = JsonSerializer.Deserialize<ConfigSetting>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (cfg == null)
                {
                    StatusMessage = "Invalid code (could not read configuration).";
                    return;
                }

                // Optional minimal sanity checks
                cfg.General ??= new GeneralSettings();
                cfg.Notification ??= new NotificationSettings();
                cfg.Theme ??= new ThemeSettings();
                cfg.TimerSettings ??= new TimerSettings();

                // Save and propagate
                _settingsService.SaveSettings(cfg);
                _hotkeyService?.ReloadHotkeys();

                StatusMessage = "Settings imported and applied.";

                // Refresh export so user can copy the normalized form
                GenerateExportCode();
            }
            catch (FormatException)
            {
                StatusMessage = "Invalid Base64 code.";
            }
            catch (JsonException)
            {
                StatusMessage = "Invalid JSON inside code.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unable to import: {ex.Message}";
            }
        }
    }
}
