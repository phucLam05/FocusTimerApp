namespace GroupThree.FocusTimerApp.Services
{
    using System.Collections.Generic;
    using System.IO;
    using System;
    using Newtonsoft.Json;
    using GroupThree.FocusTimerApp.Models;

    public interface IPlaylistStorageService
    {
        IReadOnlyList<Mp3Track> Load();
        void Save(IEnumerable<Mp3Track> tracks);
        string GetStoragePath();
        string GetLibraryFolder();
    }

    public class PlaylistStorageService : IPlaylistStorageService
    {
        private const string FolderName = "FocusTimerApp";
        private const string FileName = "playlist.json";
        private const string MusicFolderName = "Music";

        private string GetBaseFolder()
        {
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Roaming
            var dir = Path.Combine(baseDir, FolderName);
            Directory.CreateDirectory(dir);
            return dir;
        }

        public string GetStoragePath()
        {
            var dir = GetBaseFolder();
            return Path.Combine(dir, FileName);
        }

        public string GetLibraryFolder()
        {
            var dir = Path.Combine(GetBaseFolder(), MusicFolderName);
            Directory.CreateDirectory(dir);
            return dir;
        }

        public IReadOnlyList<Mp3Track> Load()
        {
            try
            {
                var path = GetStoragePath();
                if (!File.Exists(path)) return Array.Empty<Mp3Track>();
                var json = File.ReadAllText(path);
                var list = JsonConvert.DeserializeObject<List<Mp3Track>>(json) ?? new List<Mp3Track>();
                // filter non-existing files
                list.RemoveAll(t => string.IsNullOrWhiteSpace(t.FilePath) || !File.Exists(t.FilePath));
                return list;
            }
            catch
            {
                return Array.Empty<Mp3Track>();
            }
        }

        public void Save(IEnumerable<Mp3Track> tracks)
        {
            try
            {
                var path = GetStoragePath();
                var json = JsonConvert.SerializeObject(tracks, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch
            {
                // ignore for now
            }
        }
    }
}
