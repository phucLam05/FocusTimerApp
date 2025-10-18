using GroupThree.FocusTimerApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand StartCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand StopCommand { get; }

        private readonly DispatcherTimer _timer;
        private readonly TimerSetting _settings;
        private readonly TimerState _state;

        public string CurrentPhase
        {
            get => _state.CurrentPhase;
            set { _state.CurrentPhase = value; OnPropertyChanged(); }
        }
        public bool IsRunning
        {
            get => _state.IsRunning;
            set { _state.IsRunning = value; OnPropertyChanged(); }
        }
        public int TimeLeft
        {
            get => (int)_state.TimeLeft.TotalSeconds;
            set { _state.TimeLeft = TimeSpan.FromSeconds(value); OnPropertyChanged(); }
        }

        public string TimeLeftDisplay => _state.TimeLeft.ToString(@"hh\:mm\:ss");

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_state.TimeLeft.TotalSeconds > 0)
            {
                _state.TimeLeft = _state.TimeLeft.Subtract(TimeSpan.FromSeconds(1));
                OnPropertyChanged(nameof(TimeLeftDisplay));
            }
            else
            {
                _timer.Stop();
                _state.IsRunning = false;
                OnPropertyChanged(nameof(IsRunning));
                ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
                // Handle phase transitions here (e.g., switch to break)
            }
        }
        private void StartTimer()
        {
            IsRunning = true;
            _timer.Start();
            ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
            ((Commands.RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }

        private void StopTimer()
        {
            IsRunning = false;
            _timer.Stop();
            ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
            ((Commands.RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }

        private void ResetState()
        {
            CurrentPhase = "Basic";
            TimeLeft = _settings.WorkDuration;
            IsRunning = false;
            ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
            ((Commands.RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }

        public MainViewModel()
        {
            _settings = SettingManager.Load();
            _state = new TimerState
            {
                TimeLeft = TimeSpan.FromMinutes(_settings.WorkDuration)
            };
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            StartCommand = new Commands.RelayCommand(_ => StartTimer(), _ => !_state.IsRunning);
            ResetCommand = new Commands.RelayCommand(_ => ResetState());
            StopCommand = new Commands.RelayCommand(_ => StopTimer(), _ => _state.IsRunning);
            ResetState();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
