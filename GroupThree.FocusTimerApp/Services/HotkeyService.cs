using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using GroupThree.FocusTimerApp.Helper;
using GroupThree.FocusTimerApp.Models; // Nếu cần thêm using khác, tùy chỉnh

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
        private readonly SettingsService _settingsService; // Inject để load từ config
        private HwndSource _source;
        private int _currentId = 0;

        // Sự kiện: MainViewModel sẽ lắng nghe để biết hotkey nào được nhấn
        public event Action<string>? HotkeyPressed;

        public HotkeyService(Window window, SettingsService settingsService)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            _window.SourceInitialized += (s, e) =>
            {
                IntPtr handle = new WindowInteropHelper(_window).Handle;
                _source = HwndSource.FromHwnd(handle);
                _source.AddHook(HwndHook);
            };
        }

        // ==============================
        // Đăng ký hotkey
        // ==============================
        public void RegisterHotkeys()
        {
            var hotkeys = _settingsService.LoadHotkeys(); // Giả sử method này trả List<HotkeyBinding> từ JSON/config
            foreach (var binding in hotkeys)
            {
                uint modifiers = 0;
                if (binding.Modifiers.HasFlag(ModifierKeys.Control)) modifiers |= HotKeyHelpers.MOD_CONTROL;
                if (binding.Modifiers.HasFlag(ModifierKeys.Alt)) modifiers |= HotKeyHelpers.MOD_ALT;
                if (binding.Modifiers.HasFlag(ModifierKeys.Shift)) modifiers |= HotKeyHelpers.MOD_SHIFT;
                if (binding.Modifiers.HasFlag(ModifierKeys.Windows)) modifiers |= HotKeyHelpers.MOD_WIN;

                RegisterHotkey(binding.ActionName, modifiers, binding.Key);
            }
        }

        // Đăng ký từng hotkey cụ thể
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
                    // Thay Console bằng Logger nếu có: Logger.LogInfo($"Registered hotkey: {actionName} ({HotKeyHelpers.ToString(modifiers, key)})");
                    Console.WriteLine($"Registered hotkey: {actionName} ({HotKeyHelpers.ToString(modifiers, key)})");
                }
                else
                {
                    throw new InvalidOperationException($"Failed to register hotkey for {actionName}. It may be in use by another application.");
                }
            }
            catch (Exception ex)
            {
                // Thay Console bằng Logger nếu có
                Console.WriteLine($"Error registering hotkey {actionName}: {ex.Message}");
                // Có thể notify user qua event hoặc message box ở ViewModel
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
                    // Invoke trên UI thread nếu cần update UI
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

            foreach (var id in _registeredHotkeys.Keys.ToArray())
            {
                UnregisterHotKey(handle, id);
            }
            _registeredHotkeys.Clear();
        }

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