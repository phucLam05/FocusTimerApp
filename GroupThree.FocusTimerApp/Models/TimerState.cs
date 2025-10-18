namespace GroupThree.FocusTimerApp.Models
{
    public class TimerState
    {
        public TimeSpan TimeLeft { get; set; }
        public bool IsRunning { get; set; }
        public string CurrentPhase { get; set; } = "Basic"; // Basic, Pomodoro, ShortBreak, LongBreak
        public int CompletedShortBreaks { get; set; } = 0;
    }
    public enum TimerPhase
    {
        Basic,
        Pomodoro,
        ShortBreak,
        LongBreak
    }
}
