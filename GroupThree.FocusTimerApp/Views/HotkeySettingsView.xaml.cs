using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class HotkeySettingsView : System.Windows.Controls.UserControl
    {
        private const string HintText = "Press desired key combination...";

        public HotkeySettingsView()
        {
            InitializeComponent();
        }

        private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                tb.Tag = tb.Text; // keep old value in Tag
                tb.Text = string.Empty;
                tb.ToolTip = HintText;
            }
        }

        private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                tb.ToolTip = null;
                if (string.IsNullOrWhiteSpace(tb.Text) && tb.Tag is string prev && !string.IsNullOrWhiteSpace(prev))
                {
                    // restore previous if user left without input
                    tb.Text = prev;
                }
                tb.Tag = null;
            }
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox tb) return;

            // Ignore pure modifier keys as final key
            var isModifierOnly = e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin;

            ModifierKeys mods = Keyboard.Modifiers;
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin)
            {
                // wait for a non-modifier key
                e.Handled = true;
                return;
            }

            string combo = BuildComboString(mods, key);
            if (string.IsNullOrWhiteSpace(combo))
            {
                e.Handled = true;
                return;
            }

            // Try to assign and register immediately
            if (tb.DataContext is HotkeyBinding binding)
            {
                var app = System.Windows.Application.Current as App;
                var hk = app?.HotkeyServiceInstance;
                if (hk != null)
                {
                    try
                    {
                        // Temporarily unregister all and try register this one alone to validate
                        hk.UnregisterAll();

                        binding.HotkeyString = combo; // updates Key/Modifiers

                        // Register all existing hotkeys again
                        hk.RegisterHotkeys();

                        // success: update textbox and keep focus here
                        tb.Text = combo;
                        tb.ToolTip = null;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Cannot register hotkey: {combo}\n{ex.Message}", "Hotkey", MessageBoxButton.OK, MessageBoxImage.Warning);
                        // keep focus for retry, restore previous display
                        if (tb.Tag is string prev)
                        {
                            tb.Text = prev;
                        }
                    }
                }
                else
                {
                    // No HotkeyService yet
                    binding.HotkeyString = combo;
                    tb.Text = combo;
                }
            }

            e.Handled = true; // prevent ding
        }

        private static string BuildComboString(ModifierKeys mods, Key key)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (mods.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (mods.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (mods.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (mods.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            if (key != Key.None) parts.Add(key.ToString());
            return string.Join("+", parts);
        }
    }
}