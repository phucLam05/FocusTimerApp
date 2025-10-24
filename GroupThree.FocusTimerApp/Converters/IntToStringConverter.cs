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
                // If empty string, keep previous value
                if (string.IsNullOrWhiteSpace(strValue))
                {
                    return Binding.DoNothing;
                }
                
                // Try to parse the string to int
                if (int.TryParse(strValue, out int result))
                {
                    // Allow any integer value, validation will happen on Save
                    return result;
                }
                else
                {
                    // If not a valid number, keep the previous value
                    return Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }
    }
}
