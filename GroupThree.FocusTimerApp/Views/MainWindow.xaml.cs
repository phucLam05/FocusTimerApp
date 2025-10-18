using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class MainWindow : Window
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly Dictionary<int, string> _hotkeys = new();
        private int _hotkeyId = 0;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(HwndHook);

            RegisterHotkey(handle, ModifierKeys.Alt, Key.Q, "Alt + Q");
            RegisterHotkey(handle, ModifierKeys.Alt, Key.W, "Alt + W");
            RegisterHotkey(handle, ModifierKeys.Alt, Key.E, "Alt + E");
        }

        private void RegisterHotkey(IntPtr handle, ModifierKeys modifiers, Key key, string label)
        {
            _hotkeyId++;
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            RegisterHotKey(handle, _hotkeyId, (uint)modifiers, vk);
            _hotkeys[_hotkeyId] = label;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_hotkeys.ContainsKey(id))
                {
                    HotkeyText.Text = $"🔥 Bạn vừa nhấn {_hotkeys[id]}";
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            foreach (var id in _hotkeys.Keys)
            {
                UnregisterHotKey(handle, id);
            }
            base.OnClosed(e);
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
