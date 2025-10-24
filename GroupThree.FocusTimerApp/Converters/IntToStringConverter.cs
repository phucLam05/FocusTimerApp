using System;
using System.Globalization;
using System.Windows.Data;

namespace GroupThree.FocusTimerApp.Converters
{
    /// <summary>
    /// Converts between int and string for TextBox binding
    /// Handles empty strings and invalid input gracefully
    /// </summary>
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                // Try to parse the string to int
                if (int.TryParse(strValue, out int result))
                {
                    // Validate that it's positive
                    return result > 0 ? result : 1;
                }
                // If empty or invalid, return 1 as default
                return 1;
            }
            return 1;
        }
    }
}
