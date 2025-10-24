using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace GroupThree.FocusTimerApp.Services
{
    public class ThemeService : IThemeService
    {
        private bool _isDarkMode = true; // Default to dark mode

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                    ApplyTheme();
                }
            }
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        public void ApplyTheme()
        {
            var app = Application.Current;
            if (app == null) return;

            // Get the application resources
            var resources = app.Resources;

            if (IsDarkMode)
            {
                // Dark Mode Colors - Modern dark theme with purple accent
                resources["WindowBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#1a1f2e");
                resources["HeaderBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#252b3d");
                resources["SidebarBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#1e2332");
                resources["CardBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#252b3d");
                resources["PrimaryButtonColor"] = (Color)ColorConverter.ConvertFromString("#6366f1");
                resources["PrimaryButtonHoverColor"] = (Color)ColorConverter.ConvertFromString("#7c3aed");
                resources["SecondaryButtonColor"] = (Color)ColorConverter.ConvertFromString("#3f4458");
                resources["SecondaryButtonHoverColor"] = (Color)ColorConverter.ConvertFromString("#4a5068");
                resources["ProgressBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#2d3548");
                resources["ProgressForegroundColor"] = (Color)ColorConverter.ConvertFromString("#6366f1");
                resources["TextPrimaryColor"] = Colors.White;
                resources["TextSecondaryColor"] = (Color)ColorConverter.ConvertFromString("#9ca3af");
                resources["BorderColor"] = (Color)ColorConverter.ConvertFromString("#374151");
            }
            else
            {
                // Light Mode Colors - Clean and modern light theme
                resources["WindowBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#f8f9fa");
                resources["HeaderBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#ffffff");
                resources["SidebarBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#f3f4f6");
                resources["CardBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#ffffff");
                resources["PrimaryButtonColor"] = (Color)ColorConverter.ConvertFromString("#6366f1");
                resources["PrimaryButtonHoverColor"] = (Color)ColorConverter.ConvertFromString("#4f46e5");
                resources["SecondaryButtonColor"] = (Color)ColorConverter.ConvertFromString("#f3f4f6");
                resources["SecondaryButtonHoverColor"] = (Color)ColorConverter.ConvertFromString("#e5e7eb");
                resources["ProgressBackgroundColor"] = (Color)ColorConverter.ConvertFromString("#e5e7eb");
                resources["ProgressForegroundColor"] = (Color)ColorConverter.ConvertFromString("#6366f1");
                resources["TextPrimaryColor"] = (Color)ColorConverter.ConvertFromString("#1f2937");
                resources["TextSecondaryColor"] = (Color)ColorConverter.ConvertFromString("#6b7280");
                resources["BorderColor"] = (Color)ColorConverter.ConvertFromString("#e5e7eb");
            }

            // Create brushes from colors
            resources["WindowBackgroundBrush"] = new SolidColorBrush((Color)resources["WindowBackgroundColor"]);
            resources["HeaderBackgroundBrush"] = new SolidColorBrush((Color)resources["HeaderBackgroundColor"]);
            resources["SidebarBackgroundBrush"] = new SolidColorBrush((Color)resources["SidebarBackgroundColor"]);
            resources["CardBackgroundBrush"] = new SolidColorBrush((Color)resources["CardBackgroundColor"]);
            resources["PrimaryButtonBrush"] = new SolidColorBrush((Color)resources["PrimaryButtonColor"]);
            resources["PrimaryButtonHoverBrush"] = new SolidColorBrush((Color)resources["PrimaryButtonHoverColor"]);
            resources["SecondaryButtonBrush"] = new SolidColorBrush((Color)resources["SecondaryButtonColor"]);
            resources["SecondaryButtonHoverBrush"] = new SolidColorBrush((Color)resources["SecondaryButtonHoverColor"]);
            resources["ProgressBackgroundBrush"] = new SolidColorBrush((Color)resources["ProgressBackgroundColor"]);
            resources["ProgressForegroundBrush"] = new SolidColorBrush((Color)resources["ProgressForegroundColor"]);
            resources["TextPrimaryBrush"] = new SolidColorBrush((Color)resources["TextPrimaryColor"]);
            resources["TextSecondaryBrush"] = new SolidColorBrush((Color)resources["TextSecondaryColor"]);
            resources["BorderBrush"] = new SolidColorBrush((Color)resources["BorderColor"]);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
