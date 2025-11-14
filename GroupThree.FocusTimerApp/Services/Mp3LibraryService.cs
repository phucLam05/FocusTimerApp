using System.IO;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.Services
{
    public interface IMp3LibraryService
    {
        IEnumerable<Mp3Track> LoadFromFolder(string folderPath);
        bool TryReadFile(string filePath, out Mp3Track? track);
    }

    public class Mp3LibraryService : IMp3LibraryService
    {
        private static readonly string[] Extensions = new[] { ".mp3", ".m4a", ".flac", ".ogg", ".wav", ".wma" };

        public IEnumerable<Mp3Track> LoadFromFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                yield break;

            IEnumerable<string> files = Enumerable.Empty<string>();
            try
            {
                files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                  .Where(f => Extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            }
            catch
            {
                yield break;
            }

            foreach (var file in files)
            {
                if (TryReadFile(file, out var track) && track != null)
                    yield return track;
            }
        }

        public bool TryReadFile(string filePath, out Mp3Track? track)
        {
            track = null;
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                return false;

            try
            {
                using var tagFile = TagLib.File.Create(filePath);
                var tags = tagFile.Tag;
                track = new Mp3Track
                {
                    FilePath = filePath,
                    Title = string.IsNullOrWhiteSpace(tags?.Title) ? Path.GetFileNameWithoutExtension(filePath) : tags!.Title,
                    Artist = tags?.FirstPerformer,
                    Album = tags?.Album,
                    Duration = tagFile.Properties?.Duration ?? TimeSpan.Zero
                };
                return true;
            }
            catch
            {
                // fallback minimal
                track = new Mp3Track
                {
                    FilePath = filePath,
                    Title = Path.GetFileNameWithoutExtension(filePath)
                };
                return true;
            }
        }
    }
}
