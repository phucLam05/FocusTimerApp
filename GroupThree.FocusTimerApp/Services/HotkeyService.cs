using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using GroupThree.FocusTimerApp.Helper;

namespace GroupThree.FocusTimerApp.Services
{
    public class HotkeyService : IDisposable
    {
        // ==============================
        // Win32 API Import
        // ==============================
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        // ==============================
        // Fields & Constructor
        // ==============================
        private readonly Dictionary<int, string> _registeredHotkeys = new();
        private readonly Window _window;
        private HwndSource _source;
        private int _currentId = 0;

        // Sự kiện: MainViewModel sẽ lắng nghe để biết hotkey nào được nhấn
        public event Action<string>? HotkeyPressed;

        public HotkeyService(Window window)
        {
            _window = window;
            IntPtr handle = new WindowInteropHelper(_window).EnsureHandle();
            _source = HwndSource.FromHwnd(handle);
            _source.AddHook(HwndHook);
        }

        // ==============================
        // Đăng ký hotkey
        // ==============================
        public void RegisterHotkeys()
        {
            // ALT + Q → Switch Mode
            RegisterHotkey("SwitchMode", "Alt+Q");

            // ALT + P → Start / Stop
            RegisterHotkey("StartStop", "Alt+P");

            // ALT + A → Open Alarm
            RegisterHotkey("OpenAlarm", "Alt+A");
        }

        // Đăng ký từng hotkey cụ thể
        public void RegisterHotkey(string actionName, string hotkeyString)
        {
            try
            {
                var (modifiers, key) = HotKeyHelpers.Parse(hotkeyString);
                int id = ++_currentId;
                IntPtr handle = new WindowInteropHelper(_window).Handle;

                bool success = RegisterHotKey(handle, id, (uint)modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));

                if (success)
                {
                    _registeredHotkeys[id] = actionName;
                    Console.WriteLine($"✅ Registered hotkey: {actionName} ({hotkeyString})");
                }
                else
                {
                    Console.WriteLine($"⚠️ Failed to register hotkey: {hotkeyString} (maybe already in use)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error registering hotkey {actionName}: {ex.Message}");
            }
        }

        // ==============================
        // Nhận sự kiện nhấn phím toàn hệ thống
        // ==============================
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_registeredHotkeys.TryGetValue(id, out string? action))
                {
                    HotkeyPressed?.Invoke(action);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        // ==============================
        // Gỡ đăng ký tất cả hotkey khi app tắt
        // ==============================
        public void UnregisterAll()
        {
            IntPtr handle = new WindowInteropHelper(_window).Handle;
            foreach (var id in _registeredHotkeys.Keys)
            {
                UnregisterHotKey(handle, id);
            }
            _registeredHotkeys.Clear();
        }

        public void Dispose()
        {
            UnregisterAll();
            _source.RemoveHook(HwndHook);
        }
    }
}
