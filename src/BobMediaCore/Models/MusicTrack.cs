using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
 
namespace BobMediaPlayer.Library
{
    public class AudioLibraryTrack
    {
        public string Path { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        
        public string DurationFormatted => Duration.TotalHours >= 1 
            ? Duration.ToString(@"hh\:mm\:ss") 
            : Duration.ToString(@"mm\:ss");
    }

    public class AlbumInfo : INotifyPropertyChanged
    {
        public string AlbumName { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public BitmapImage AlbumArt { get; set; } = new BitmapImage();
        public List<string> TrackPaths { get; set; } = new List<string>();
        public List<AudioLibraryTrack> Tracks { get; set; } = new List<AudioLibraryTrack>();
        public string Artist => ArtistName;

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
