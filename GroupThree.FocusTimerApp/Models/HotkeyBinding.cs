using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;

// Alias để tránh nhầm Key property với System.Windows.Input.Key
using KeyEnum = System.Windows.Input.Key;

namespace GroupThree.FocusTimerApp.Models
{
    public class HotkeyBinding : INotifyPropertyChanged
    {
        private string _actionName = string.Empty;
        private string _key = string.Empty;
        private string _modifiers = string.Empty;
        private string _description = string.Empty;
        private bool _isRegistered = false;

        public string ActionName
        {
            get => _actionName;
            set => SetField(ref _actionName, value);
        }

        public string Key
        {
            get => _key;
            set
            {
                if (SetField(ref _key, value))
                    OnPropertyChanged(nameof(ParsedKey));
            }
        }

        public string Modifiers
        {
            get => _modifiers;
            set
            {
                if (SetField(ref _modifiers, value))
                    OnPropertyChanged(nameof(ParsedModifiers));
            }
        }

        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        [JsonIgnore]
        public KeyEnum ParsedKey
        {
            get
            {
                return Enum.TryParse(Key, true, out KeyEnum parsed)
                    ? parsed
                    : KeyEnum.None;
            }
        }

        [JsonIgnore]
        public ModifierKeys ParsedModifiers
        {
            get
            {
                ModifierKeys modifiers = ModifierKeys.None;
                foreach (var part in (Modifiers ?? "")
                         .Split('+', StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (part.Trim().ToLower())
                    {
                        case "ctrl":
                        case "control":
                            modifiers |= ModifierKeys.Control;
                            break;
                        case "alt":
                            modifiers |= ModifierKeys.Alt;
                            break;
                        case "shift":
                            modifiers |= ModifierKeys.Shift;
                            break;
                        case "win":
                        case "windows":
                            modifiers |= ModifierKeys.Windows;
                            break;
                    }
                }
                return modifiers;
            }
        }

        [JsonIgnore]
        public bool IsRegistered
        {
            get => _isRegistered;
            set => SetField(ref _isRegistered, value);
        }

        // Chuỗi hiển thị đầy đủ dạng "Ctrl+Alt+S"
        [JsonIgnore]
        public string HotkeyString
        {
            get
            {
                var parts = new List<string>();
                if (ParsedModifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
                if (ParsedModifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
                if (ParsedModifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
                if (ParsedModifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
                if (ParsedKey != KeyEnum.None) parts.Add(ParsedKey.ToString());
                return string.Join("+", parts);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Key = string.Empty;
                    Modifiers = string.Empty;
                    return;
                }

                var parts = value.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 0)
                {
                    Key = string.Empty;
                    Modifiers = string.Empty;
                    return;
                }

                // Phần cuối là Key, phần trước là Modifier
                var keyPart = parts[^1];
                Key = keyPart;

                if (parts.Length > 1)
                    Modifiers = string.Join("+", parts[..^1]);
                else
                    Modifiers = string.Empty;

                OnPropertyChanged(nameof(HotkeyString));
            }
        }

        public override string ToString()
        {
            List<string> parts = new();
            if (ParsedModifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (ParsedModifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (ParsedModifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (ParsedModifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            if (ParsedKey != KeyEnum.None) parts.Add(ParsedKey.ToString());
            return string.Join("+", parts);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
