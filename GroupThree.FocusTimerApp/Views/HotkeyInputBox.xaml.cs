using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class HotkeyInputBox : Window
    {
        private ModifierKeys _currentModifiers = ModifierKeys.None;
        private Key _currentKey = Key.None;
        
        public string ResultKey { get; private set; } = string.Empty;
        public string ResultModifiers { get; private set; } = string.Empty;
        
        public HotkeyInputBox(string actionName, string currentKey, string currentModifiers)
        {
            InitializeComponent();
            
            ActionNameText.Text = $"Action: {actionName}";
            
            // Parse current hotkey
            ParseCurrentHotkey(currentKey, currentModifiers);
            UpdateDisplay();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus the window to capture key events
            this.Focus();
        }
        
        private void ParseCurrentHotkey(string key, string modifiers)
        {
            // Parse key
            if (!string.IsNullOrEmpty(key))
            {
                if (System.Enum.TryParse(key, true, out Key parsedKey))
                {
                    _currentKey = parsedKey;
                }
            }
            
            // Parse modifiers
            _currentModifiers = ModifierKeys.None;
            if (!string.IsNullOrEmpty(modifiers))
            {
                foreach (var part in modifiers.Split('+', System.StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (part.Trim().ToLower())
                    {
                        case "ctrl":
                        case "control":
                            _currentModifiers |= ModifierKeys.Control;
                            break;
                        case "alt":
                            _currentModifiers |= ModifierKeys.Alt;
                            break;
                        case "shift":
                            _currentModifiers |= ModifierKeys.Shift;
                            break;
                        case "win":
                        case "windows":
                            _currentModifiers |= ModifierKeys.Windows;
                            break;
                    }
                }
            }
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            
            // Handle special keys
            if (key == Key.Escape)
            {
                DialogResult = false;
                Close();
                return;
            }
            
            if (key == Key.Back || key == Key.Delete)
            {
                ClearHotkey();
                return;
            }
            
            // Ignore modifier-only presses
            if (IsModifierKey(key))
            {
                return;
            }
            
            // Capture modifiers
            _currentModifiers = Keyboard.Modifiers;
            _currentKey = key;
            
            UpdateDisplay();
        }
        
        private bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt ||
                   key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LWin || key == Key.RWin;
        }
        
        private void UpdateDisplay()
        {
            var parts = new List<string>();
            
            if (_currentModifiers.HasFlag(ModifierKeys.Control))
                parts.Add("Ctrl");
            if (_currentModifiers.HasFlag(ModifierKeys.Alt))
                parts.Add("Alt");
            if (_currentModifiers.HasFlag(ModifierKeys.Shift))
                parts.Add("Shift");
            if (_currentModifiers.HasFlag(ModifierKeys.Windows))
                parts.Add("Win");
            
            if (_currentKey != Key.None)
                parts.Add(_currentKey.ToString());
            
            HotkeyDisplay.Text = parts.Count > 0 ? string.Join(" + ", parts) : "(Not set)";
        }
        
        private void ClearHotkey()
        {
            _currentKey = Key.None;
            _currentModifiers = ModifierKeys.None;
            UpdateDisplay();
        }
        
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearHotkey();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Build result strings
            var modifierParts = new List<string>();
            
            if (_currentModifiers.HasFlag(ModifierKeys.Control))
                modifierParts.Add("Ctrl");
            if (_currentModifiers.HasFlag(ModifierKeys.Alt))
                modifierParts.Add("Alt");
            if (_currentModifiers.HasFlag(ModifierKeys.Shift))
                modifierParts.Add("Shift");
            if (_currentModifiers.HasFlag(ModifierKeys.Windows))
                modifierParts.Add("Win");
            
            ResultModifiers = string.Join("+", modifierParts);
            ResultKey = _currentKey != Key.None ? _currentKey.ToString() : string.Empty;
            
            DialogResult = true;
            Close();
        }
    }
}
