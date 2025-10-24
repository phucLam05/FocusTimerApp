using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;

// 👇 Thêm dòng này
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
        private string _displayText = string.Empty;

        // Add constructor to ensure DisplayText is initialized
        public HotkeyBinding()
        {
            // Initialize DisplayText when object is created
            UpdateDisplayText();
        }

        public string ActionName { get => _actionName; set => SetField(ref _actionName, value); }
        
        public string Key 
        { 
            get => _key; 
            set 
            { 
                if (SetField(ref _key, value)) 
                {
                    UpdateDisplayText();
                    OnPropertyChanged(nameof(ParsedKey));
                    OnPropertyChanged(nameof(HotkeyString));
                } 
            } 
        }
        
        public string Modifiers 
        { 
            get => _modifiers; 
            set 
            { 
                if (SetField(ref _modifiers, value)) 
                {
                    UpdateDisplayText();
                    OnPropertyChanged(nameof(ParsedModifiers));
                    OnPropertyChanged(nameof(HotkeyString));
                } 
            } 
        }
        
        public string Description { get => _description; set => SetField(ref _description, value); }

        /// <summary>
        /// Simple string property for UI display - THIS WILL WORK!
        /// </summary>
        [JsonIgnore]
        public string DisplayText
        {
            get 
            {
                // If _displayText is empty, recalculate it
                if (string.IsNullOrEmpty(_displayText))
                {
                    UpdateDisplayText();
                }
                return _displayText;
            }
            set => SetField(ref _displayText, value);
        }

        private void UpdateDisplayText()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Modifiers))
            {
                parts.Add(Modifiers);
            }
            if (!string.IsNullOrEmpty(Key))
            {
                parts.Add(Key);
            }
            var newDisplayText = string.Join("+", parts);
            if (_displayText != newDisplayText)
            {
                _displayText = newDisplayText;
                OnPropertyChanged(nameof(DisplayText));
                System.Diagnostics.Debug.WriteLine($"[HotkeyBinding] DisplayText updated to: '{_displayText}' for {ActionName}");
            }
        }

        [JsonIgnore]
        public KeyEnum ParsedKey
        {
            get
            {
                return Enum.TryParse(Key, true, out KeyEnum parsed) ? parsed : KeyEnum.None;
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
        public bool IsRegistered { get => _isRegistered; set => SetField(ref _isRegistered, value); }

        // Combined string for UI like "Ctrl+Alt+P". When set, parse into Modifiers and Key.
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

                // last part considered Key, others modifiers
                var keyPart = parts[^1];
                Key = keyPart;

                if (parts.Length > 1)
                {
                    Modifiers = string.Join("+", parts[..^1]);
                }
                else
                {
                    Modifiers = string.Empty;
                }

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
