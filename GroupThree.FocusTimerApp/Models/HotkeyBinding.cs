using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.Models
{
    public class HotkeyBinding
    {
        public string ActionName { get; set; } = string.Empty;
        public string KeyName { get; set; } = string.Empty;
        public string Modifiers { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [JsonIgnore]
        public Key ParsedKey => (Key)Enum.Parse(typeof(Key), KeyName, true);

        [JsonIgnore]
        public ModifierKeys ParsedModifiers
        {
            get
            {
                ModifierKeys modifiers = ModifierKeys.None;
                foreach (var part in Modifiers.Split('+', StringSplitOptions.RemoveEmptyEntries))
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

        public override string ToString()
        {
            List<string> parts = new();
            if (ParsedModifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (ParsedModifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (ParsedModifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (ParsedModifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            if (ParsedKey != Key.None) parts.Add(ParsedKey.ToString());
            return string.Join("+", parts);
        }
    }
}
