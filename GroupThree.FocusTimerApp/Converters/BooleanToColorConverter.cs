using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GroupThree.FocusTimerApp.Converters
{
    /// <summary>
    /// Converts boolean IsRegistered value to a color brush
    /// Green for registered, Orange for not registered
    /// </summary>
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRegistered)
            {
                return isRegistered 
                    ? new SolidColorBrush(Color.FromRgb(34, 197, 94))  // Green
                    : new SolidColorBrush(Color.FromRgb(251, 146, 60)); // Orange
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
