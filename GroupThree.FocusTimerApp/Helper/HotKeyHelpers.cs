using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GroupThree.FocusTimerApp.Helper
{
    internal class HotKeyHelpers
    {
        // Hằng số WinAPI cho các phím modifier
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        /// <summary>
        /// Parse chuỗi phím tắt thành các thành phần modifier và phím chính.
        /// </summary>
        public static (uint modifier, Key key) ParseHotkey(string hotkeyString)
        {
            if (string.IsNullOrWhiteSpace(hotkeyString))
                throw new ArgumentException("Hotkey string cannot be empty");

            string[] parts = hotkeyString.Split('+', StringSplitOptions.RemoveEmptyEntries);

            uint modifier = 0;
            Key key = Key.None;

            foreach (string part in parts.Select(p => p.Trim()))
            {
                switch (part.ToUpperInvariant())
                {
                    case "CTRL":
                    case "CONTROL":
                        modifier |= MOD_CONTROL;
                        break;
                    case "ALT":
                        modifier |= MOD_ALT;
                        break;
                    case "SHIFT":
                        modifier |= MOD_SHIFT;
                        break;
                    case "WIN":
                    case "WINDOWS":
                        modifier |= MOD_WIN;
                        break;
                    default:
                        key = (Key)Enum.Parse(typeof(Key), part, true);
                        break;
                }
            }

            return (modifier, key);
        }

        /// <summary>
        /// Chuyển ngược lại từ Key + modifier thành chuỗi "Ctrl+Alt+S".
        /// </summary>
        //public static string ToString(uint modifier, Key key)
        //{
        //    List<string> parts = new();

        //    if ((modifier & MOD_CONTROL) != 0) parts.Add("Ctrl");
        //    if ((modifier & MOD_ALT) != 0) parts.Add("Alt");
        //    if ((modifier & MOD_SHIFT) != 0) parts.Add("Shift");
        //    if ((modifier & MOD_WIN) != 0) parts.Add("Win");

        //    parts.Add(key.ToString().ToUpperInvariant());
        //    return string.Join("+", parts);
        //}
    }
}

