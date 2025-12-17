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
                    // This is not necessary if SetCurrentFile is called right after AddDirectory
                    // but it's safe to keep for direct calls.
                    // currentIndex = 0;
                }
            }
        }

        public void AddDirectory(string directoryPath, string[] extensions)
        {
            try
            {
                // Note: It's better to avoid catch-all blocks. At least log the error.
                var mediaFiles = Directory.GetFiles(directoryPath)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                    .OrderBy(f => f)
                    .ToList();

                // Only clear the playlist if it was empty, otherwise, the LoadMedia
                // in MainWindow should call Clear() before this. Assuming LoadMedia
                // already clears it, this is fine.

                foreach (var file in mediaFiles)
                {
                    AddFile(file);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Silently ignore if the directory doesn't exist.
            }
            catch (Exception)
            {
                // Handle other exceptions like access denied, etc.
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
                // If the file is not in the list (e.g., opened from outside the directory),
                // add it and set the index.
                AddFile(filePath);
                currentIndex = files.Count - 1;
            }
        }

        public string? Next()
        {
            if (files.Count == 0) return null;

            // Circular logic: Moves to the next index, wrapping to 0 if at the end.
            currentIndex = (currentIndex + 1) % files.Count;
            return files[currentIndex];
        }

        public string? Previous()
        {
            if (files.Count == 0) return null;

            // *** FIX: Apply Circular Logic to Previous() ***
            // This is the correct, common way to get the previous index
            // while wrapping from 0 to the last index (Count - 1).
            currentIndex = (currentIndex - 1 + files.Count) % files.Count;
            // **********************************************

            return files[currentIndex];
        }

        public bool HasNext()
        {
            // If the playlist has files, there is always a "next" file using circular navigation.
            // If you want non-circular navigation, use the original logic: return files.Count > 0 && currentIndex < files.Count - 1;
            return files.Count > 0;
        }

        public bool HasPrevious()
        {
            // If the playlist has files, there is always a "previous" file using circular navigation.
            // If you want non-circular navigation, use the original logic: return files.Count > 0 && currentIndex > 0;
            return files.Count > 0;
        }
    }
}
