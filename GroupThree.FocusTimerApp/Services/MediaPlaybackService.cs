namespace GroupThree.FocusTimerApp.Services
{
    using System;
    using System.IO;

    public interface IMediaPlaybackService
    {
        void Play(string filePath);
        void Stop();
        void Pause();
        bool IsPlaying { get; }
        string? CurrentFile { get; }
    }

    public class MediaPlaybackService : IMediaPlaybackService, IDisposable
    {
        private readonly System.Windows.Media.MediaPlayer _player = new();
        private bool _isPlaying;
        public bool IsPlaying => _isPlaying;
        public string? CurrentFile { get; private set; }

        public void Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;
            try
            {
                _player.Open(new Uri(filePath));
                _player.MediaEnded -= OnEnded;
                _player.MediaEnded += OnEnded;
                _player.Play();
                _isPlaying = true;
                CurrentFile = filePath;
            }
            catch
            {
                _isPlaying = false;
            }
        }

        public void Pause()
        {
            try { _player.Pause(); _isPlaying = false; } catch { }
        }

        public void Stop()
        {
            try { _player.Stop(); _isPlaying = false; } catch { }
        }

        private void OnEnded(object? s, EventArgs e)
        {
            _isPlaying = false;
        }

        public void Dispose()
        {
            try
            {
                _player.Close();
            }
            catch { }
        }
    }
}
