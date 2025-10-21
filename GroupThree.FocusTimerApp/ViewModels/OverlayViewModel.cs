namespace GroupThree.FocusTimerApp.ViewModels
{
    public class OverlayViewModel : ViewModelBase
    {
        private string _time = "00:00:00";
        public string Time { get => _time; set => SetProperty(ref _time, value); }

        private double _progress = 0;
        public double Progress { get => _progress; set => SetProperty(ref _progress, value); }
    }
}
