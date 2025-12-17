using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BobMediaPlayer.Library
{
    public class LibrarySettings
    {
        public List<string> Folders { get; set; } = new List<string>();
    }

    public static class LibrarySettingsStore
    {
        private static readonly string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BobMediaPlayer");
        private static readonly string SettingsPath = Path.Combine(AppDir, "library.json");

        public static List<string> GetDefaultFolders()
        {
            var list = new List<string>();
            var music = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            if (Directory.Exists(music)) list.Add(music);
            if (Directory.Exists(desktop)) list.Add(desktop);
            if (Directory.Exists(downloads)) list.Add(downloads);
            return list;
        }

        public static LibrarySettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var data = JsonSerializer.Deserialize<LibrarySettings>(json);
                    if (data != null && data.Folders != null && data.Folders.Count > 0)
                        return data;
                }
            }
            catch { }
            return new LibrarySettings { Folders = GetDefaultFolders() };
        }

        public static void Save(LibrarySettings settings)
        {
            try
            {
                Directory.CreateDirectory(AppDir);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch { }
        }
    }
}
