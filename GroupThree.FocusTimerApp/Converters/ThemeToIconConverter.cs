using System;
using System.Globalization;
using System.Windows.Data;

namespace GroupThree.FocusTimerApp.Converters
{
    public class ThemeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDarkMode)
            {
                // If dark mode, show sun icon geometry (to switch to light)
                // If light mode, show moon icon geometry (to switch to dark)
                if (isDarkMode)
                {
                    // Sun icon - simple circle with rays
                    return "M12,2A1,1 0 0,1 13,3V5A1,1 0 0,1 11,5V3A1,1 0 0,1 12,2M19.07,4.93A1,1 0 0,1 20.48,6.34L19.07,7.76A1,1 0 1,1 17.66,6.34L19.07,4.93M22,11A1,1 0 0,1 23,12A1,1 0 0,1 22,13H20A1,1 0 0,1 19,12A1,1 0 0,1 20,11H22M4,11A1,1 0 0,1 5,12A1,1 0 0,1 4,13H2A1,1 0 0,1 1,12A1,1 0 0,1 2,11H4M6.34,6.34A1,1 0 1,1 7.76,7.76L6.34,19.07A1,1 0 1,1 4.93,17.66L6.34,6.34M12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7M12,22A1,1 0 0,1 11,21V19A1,1 0 0,1 13,19V21A1,1 0 0,1 12,22M17.66,17.66A1,1 0 1,1 19.07,19.07L17.66,20.48A1,1 0 1,1 16.24,19.07L17.66,17.66Z";
                }
                else
                {
                    // Moon icon
                    return "M17.75,4.09L15.22,6.03L16.13,9.09L13.5,7.28L10.87,9.09L11.78,6.03L9.25,4.09L12.44,4L13.5,1L14.56,4L17.75,4.09M21.25,11L19.61,12.25L20.2,14.23L18.5,13.06L16.8,14.23L17.39,12.25L15.75,11L17.81,10.95L18.5,9L19.19,10.95L21.25,11M18.97,15.95C19.8,15.87 20.69,17.05 20.16,17.8C19.84,18.25 19.5,18.67 19.08,19.07C15.17,23 8.84,23 4.94,19.07C1.03,15.17 1.03,8.83 4.94,4.93C5.34,4.53 5.76,4.17 6.21,3.85C6.96,3.32 8.14,4.21 8.06,5.04C7.79,7.9 8.75,10.87 10.95,13.06C13.14,15.26 16.1,16.22 18.97,15.95M17.33,17.97C14.5,17.81 11.7,16.64 9.53,14.5C7.36,12.31 6.2,9.5 6.04,6.68C3.23,9.82 3.34,14.64 6.35,17.66C9.37,20.67 14.19,20.78 17.33,17.97Z";
                }
            }
            // Default to sun
            return "M12,2A1,1 0 0,1 13,3V5A1,1 0 0,1 11,5V3A1,1 0 0,1 12,2M19.07,4.93A1,1 0 0,1 20.48,6.34L19.07,7.76A1,1 0 1,1 17.66,6.34L19.07,4.93M22,11A1,1 0 0,1 23,12A1,1 0 0,1 22,13H20A1,1 0 0,1 19,12A1,1 0 0,1 20,11H22M4,11A1,1 0 0,1 5,12A1,1 0 0,1 4,13H2A1,1 0 0,1 1,12A1,1 0 0,1 2,11H4M6.34,6.34A1,1 0 1,1 7.76,7.76L6.34,19.07A1,1 0 1,1 4.93,17.66L6.34,6.34M12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7M12,22A1,1 0 0,1 11,21V19A1,1 0 0,1 13,19V21A1,1 0 0,1 12,22M17.66,17.66A1,1 0 1,1 19.07,19.07L17.66,20.48A1,1 0 1,1 16.24,19.07L17.66,17.66Z";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
