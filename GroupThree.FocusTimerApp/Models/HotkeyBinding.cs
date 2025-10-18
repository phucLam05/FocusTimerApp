namespace FocusTimerApp.Models
{
    public class HotkeyBinding
    {
        public string ActionName { get; set; } = string.Empty;  // Ví dụ: "StartPause", "OpenSettings"
        public Key Key { get; set; }                            // Phím chính
        public ModifierKeys Modifiers { get; set; }             // Ctrl, Alt, Shift, Win

        public HotkeyBinding() { }

        public HotkeyBinding(string action, Key key, ModifierKeys modifiers)
        {
            ActionName = action;
            Key = key;
            Modifiers = modifiers;
        }

        public override string ToString()
        {
            return $"{Modifiers}+{Key}";
        }
    }
}
