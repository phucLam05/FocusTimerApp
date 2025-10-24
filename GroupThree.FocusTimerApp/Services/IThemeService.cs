using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GroupThree.FocusTimerApp.Services
{
    public interface IThemeService : INotifyPropertyChanged
    {
        bool IsDarkMode { get; set; }
        void ToggleTheme();
        void ApplyTheme();
    }
}
