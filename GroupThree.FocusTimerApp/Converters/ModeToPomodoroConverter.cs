using System;
using System.Globalization;
using System.Windows.Data;

namespace GroupThree.FocusTimerApp.Converters
{
    public class ModeToPomodoroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string mode)
            {
                return string.Equals(mode, "Pomodoro", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                return "Pomodoro";
            }
            return Binding.DoNothing;
        }
    }
}
