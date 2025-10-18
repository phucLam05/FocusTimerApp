using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using GroupThree.FocusTimerApp.Helper;
using GroupThree.FocusTimerApp.Models;

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
        // Fields
        // ==============================
        private readonly Dictionary<int, string> _registeredHotkeys = new();
        private readonly Window _window;
        private readonly SettingsService _settingsService;
        private HwndSource? _source;
        private int _currentId = 0;

        // Sự kiện để ViewModel hoặc MainWindow lắng nghe
        public event Action<string>? HotkeyPressed;

        // ==============================
        // Constructor
        // ==============================
        public HotkeyService(Window window, SettingsService settingsService)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            IntPtr handle = new WindowInteropHelper(_window).EnsureHandle();
            _source = HwndSource.FromHwnd(handle);
            _source.AddHook(HwndHook);
        }

        // ==============================
        // Đăng ký tất cả hotkey từ config
        // ==============================
        public void RegisterHotkeys()
        {
            var hotkeys = _settingsService.LoadHotkeys();
            if (hotkeys == null || hotkeys.Count == 0)
            {
                Console.WriteLine("No hotkeys found to register.");
                return;
            }

            foreach (var binding in hotkeys)
            {
                try
                {
                    uint modifiers = 0;
                    if (binding.ParsedModifiers.HasFlag(ModifierKeys.Control)) modifiers |= HotKeyHelpers.MOD_CONTROL;
                    if (binding.ParsedModifiers.HasFlag(ModifierKeys.Alt)) modifiers |= HotKeyHelpers.MOD_ALT;
                    if (binding.ParsedModifiers.HasFlag(ModifierKeys.Shift)) modifiers |= HotKeyHelpers.MOD_SHIFT;
                    if (binding.ParsedModifiers.HasFlag(ModifierKeys.Windows)) modifiers |= HotKeyHelpers.MOD_WIN;

                    RegisterHotkey(binding.ActionName, modifiers, binding.ParsedKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to register hotkey {binding.ActionName}: {ex.Message}");
                }
            }
        }

        // ==============================
        // Đăng ký 1 hotkey cụ thể
        // ==============================
        private void RegisterHotkey(string actionName, uint modifiers, Key key)
        {
            try
            {
                int id = ++_currentId;
                IntPtr handle = new WindowInteropHelper(_window).EnsureHandle();

                bool success = RegisterHotKey(handle, id, modifiers, (uint)KeyInterop.VirtualKeyFromKey(key));
                if (success)
                {
                    _registeredHotkeys[id] = actionName;
                    Console.WriteLine($"Registered hotkey: {actionName} ({HotKeyHelpers.ToString(modifiers, key)})");
                }
                else
                {
                    Console.WriteLine($"Could not register hotkey: {actionName} ({HotKeyHelpers.ToString(modifiers, key)}) - already in use?");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering hotkey {actionName}: {ex.Message}");
            }
        }

        // ==============================
        // Lắng nghe sự kiện WM_HOTKEY từ Windows
        // ==============================
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_registeredHotkeys.TryGetValue(id, out string? action))
                {
                    _window.Dispatcher.Invoke(() => HotkeyPressed?.Invoke(action));
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
            if (handle == IntPtr.Zero) return;

            foreach (var id in _registeredHotkeys.Keys.ToList())
            {
                UnregisterHotKey(handle, id);
            }

            _registeredHotkeys.Clear();
            Console.WriteLine("All hotkeys unregistered.");
        }

        // ==============================
        // Dispose cleanup
        // ==============================
        public void Dispose()
        {
            UnregisterAll();

            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source = null;
            }

            GC.SuppressFinalize(this);
        }
        public void ReloadHotkeys() //không cần tạo mới HotkeyService, mà chỉ gọi ReloadHotkeys() là toàn bộ hotkey được cập nhật ngay
        {
            UnregisterAll();   // Gỡ tất cả hotkey cũ
            RegisterHotkeys(); // Đăng ký lại theo config mới
        }

    }
}
