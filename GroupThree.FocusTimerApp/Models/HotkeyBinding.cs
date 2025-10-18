using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.Models
{
    public class HotkeyBinding
    {
        public string ActionName { get; set; } = string.Empty; // Ví dụ: "StartPause", "OpenSettings"
        public Key Key { get; set; } // Phím chính
        public ModifierKeys Modifiers { get; set; } // Ctrl, Alt, Shift, Win

        public HotkeyBinding() { }

        public HotkeyBinding(string action, Key key, ModifierKeys modifiers)
        {
            ActionName = action;
            Key = key;
            Modifiers = modifiers;
        }

        public override string ToString()
        {
            List<string> parts = new();
            if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            if (Key != Key.None) parts.Add(Key.ToString());
            return string.Join("+", parts);
        }
    }
}