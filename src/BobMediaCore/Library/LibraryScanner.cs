using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BobMediaPlayer.Library;

namespace BobMediaPlayer
{
    public static class LibraryScanner
    {
        private static readonly string[] AudioExtensions = new[] { ".mp3", ".wav", ".m4a", ".wma", ".flac", ".aac", ".ogg" };

        public static async Task<List<AudioLibraryTrack>> ScanDefaultAsync()
        {
            var roots = new List<string?>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + "Downloads"
            };
            var existingRoots = roots.Where(r => !string.IsNullOrWhiteSpace(r) && Directory.Exists(r!)).Select(r => r!);
            return await ScanAsync(existingRoots);
        }

        public static async Task<List<AudioLibraryTrack>> ScanAsync(IEnumerable<string> roots)
        {
            return await Task.Run(() =>
            {
                var results = new List<AudioLibraryTrack>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var root in roots)
                {
                    try
                    {
                        foreach (var file in EnumerateFilesSafe(root))
                        {
                            var ext = Path.GetExtension(file).ToLowerInvariant();
                            if (!AudioExtensions.Contains(ext)) continue;
                            if (!seen.Add(file)) continue;
                            try
                            {
                                var track = ReadTrack(file);
                                results.Add(track);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                return results.OrderBy(t => t.Artist).ThenBy(t => t.Album).ThenBy(t => t.Title).ToList();
            });
        }

        private static IEnumerable<string> EnumerateFilesSafe(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                IEnumerable<string> subdirs = Array.Empty<string>();
                IEnumerable<string> files = Array.Empty<string>();
                try { subdirs = Directory.EnumerateDirectories(dir); } catch { }
                try { files = Directory.EnumerateFiles(dir); } catch { }
                foreach (var s in subdirs) stack.Push(s);
                foreach (var f in files) yield return f;
            }
        }

        private static AudioLibraryTrack ReadTrack(string path)
        {
            try
            {
                var f = TagLib.File.Create(path);
                var title = !string.IsNullOrWhiteSpace(f.Tag.Title) ? f.Tag.Title : System.IO.Path.GetFileNameWithoutExtension(path);
                string artist = "";
                if (f.Tag.Performers != null && f.Tag.Performers.Length > 0) artist = string.Join(", ", f.Tag.Performers);
                else if (f.Tag.AlbumArtists != null && f.Tag.AlbumArtists.Length > 0) artist = string.Join(", ", f.Tag.AlbumArtists);
                var album = f.Tag.Album ?? string.Empty;
                var duration = f.Properties?.Duration ?? TimeSpan.Zero;
                return new AudioLibraryTrack
                {
                    Path = path,
                    Title = title,
                    Artist = string.IsNullOrWhiteSpace(artist) ? "Unknown Artist" : artist,
                    Album = string.IsNullOrWhiteSpace(album) ? "" : album,
                    Duration = duration
                };
            }
            catch
            {
                return new AudioLibraryTrack
                {
                    Path = path,
                    Title = System.IO.Path.GetFileNameWithoutExtension(path),
                    Artist = "Unknown Artist",
                    Album = string.Empty,
                    Duration = TimeSpan.Zero
                };
            }
        }
    }
}
