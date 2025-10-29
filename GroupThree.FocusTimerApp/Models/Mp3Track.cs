namespace GroupThree.FocusTimerApp.Models
{
    using System;

    public class Mp3Track
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public TimeSpan Duration { get; set; }

        public override string ToString() => string.IsNullOrWhiteSpace(Title) ? FilePath : Title;
    }
}
