using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BobMediaPlayer
{
    public class Playlist
    {
        private List<string> files = new List<string>();
        private int currentIndex = -1;

        public int Count => files.Count;
        public int CurrentIndex => currentIndex;
        public string? CurrentFile => currentIndex >= 0 && currentIndex < files.Count ? files[currentIndex] : null;

        public void Clear()
        {
            files.Clear();
            currentIndex = -1;
        }

        public void AddFile(string filePath)
        {
            if (!files.Contains(filePath))
            {
                files.Add(filePath);
                if (currentIndex == -1)
                {
                    currentIndex = 0;
                }
            }
        }

        public void AddDirectory(string directoryPath, string[] extensions)
        {
            try
            {
                var mediaFiles = Directory.GetFiles(directoryPath)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                    .OrderBy(f => f)
                    .ToList();

                foreach (var file in mediaFiles)
                {
                    AddFile(file);
                }
            }
            catch
            {
                // i think to catch errors
            }
        }

        public void SetCurrentFile(string filePath)
        {
            int index = files.IndexOf(filePath);
            if (index >= 0)
            {
                currentIndex = index;
            }
            else
            {
                AddFile(filePath);
                currentIndex = files.Count - 1;
            }
        }

        public string? Next()
        {
            if (files.Count == 0) return null;
            
            currentIndex = (currentIndex + 1) % files.Count;
            return files[currentIndex];
        }

        public string? Previous()
        {
            if (files.Count == 0) return null;
            
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = files.Count - 1;
            }
            return files[currentIndex];
        }

        public bool HasNext()
        {
            return files.Count > 0 && currentIndex < files.Count - 1;
        }

        public bool HasPrevious()
        {
            return files.Count > 0 && currentIndex > 0;
        }
    }
}
