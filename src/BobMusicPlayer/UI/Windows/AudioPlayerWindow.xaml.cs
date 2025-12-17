using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAudio.Wave;
using BobMediaPlayer.Library;
using System.ComponentModel;

namespace BobMediaPlayer
{
    public partial class AudioPlayerWindow : Window, INotifyPropertyChanged
    {
        private readonly AudioPlaybackManager _playbackManager = AudioPlaybackManager.Instance;
        private MusicLibraryWindow? _libraryWindow;
        private IWavePlayer? wavePlayer;
        private AudioFileReader? audioFileReader;
        private DispatcherTimer timer;
        private bool isPlaying = false;
        private bool isDraggingSlider = false;
        private string audioFilePath;
        private Playlist playlist = new Playlist();
        private bool isSwitchingTrack = false;
        private bool suppressPlaybackStopped = false;

        public AudioPlayerWindow(string? filePath = null)
        {
            InitializeComponent();
            this.Topmost = true;
            SetupPlaybackManagerBindings();

            if (!string.IsNullOrEmpty(filePath))
            {
                LoadAudioFile(filePath);
            }

            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void SetupPlaybackManagerBindings()
        {
            // Bind to AudioPlaybackManager events
            _playbackManager.PropertyChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(_playbackManager.IsPlaying):
                            UpdatePlayPauseButton();
                            break;
                        case nameof(_playbackManager.Position):
                            UpdateTimeline();
                            break;
                        case nameof(_playbackManager.Duration):
                            UpdateDuration();
                            break;
                        case nameof(_playbackManager.CurrentTrack):
                            UpdateCurrentTrackDisplay();
                            break;
                    }
                });
            };

            _playbackManager.PlaybackStateChanged += (s, e) => Dispatcher.Invoke(UpdatePlayPauseButton);
            _playbackManager.PositionChanged += (s, e) => Dispatcher.Invoke(UpdateTimeline);
            _playbackManager.TrackChanged += (s, e) => Dispatcher.Invoke(UpdateCurrentTrackDisplay);
        }

        private void LoadAudioFile(string filePath)
        {
            var track = new AudioLibraryTrack
            {
                Path = filePath,
                Title = Path.GetFileNameWithoutExtension(filePath),
                Artist = "Unknown Artist"
            };

            // Set up queue with files from same directory
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null)
            {
                var audioExtensions = new[] { ".mp3", ".wav", ".m4a", ".wma" };
                var allFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Where(f => audioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .Select(f => new AudioLibraryTrack
                    {
                        Path = f,
                        Title = Path.GetFileNameWithoutExtension(f),
                        Artist = "Unknown Artist"
                    })
                    .ToList();

                var currentTrackIndex = allFiles.FindIndex(t => t.Path == filePath);
                _playbackManager.SetQueue(allFiles, currentTrackIndex);
            }
            else
            {
                _playbackManager.SetQueue(new List<AudioLibraryTrack> { track }, 0);
            }

            _playbackManager.LoadTrackAsync(track).ContinueWith(_ => _playbackManager.Play());
        }

        private void UpdatePlayPauseButton()
        {
            var playPauseButton = FindName("PlayPauseButton") as Button;
            if (playPauseButton != null)
            {
                playPauseButton.Content = _playbackManager.IsPlaying ? "[ PAUSE ]" : "[ PLAY ]";
            }
        }

        private void UpdateTimeline()
        {
            var timeline = FindName("TimelineSlider") as Slider;
            var currentTime = FindName("CurrentTimeText") as TextBlock;
            
            if (timeline != null && currentTime != null)
            {
                if (!isDraggingSlider)
                {
                    timeline.Value = _playbackManager.Position;
                    currentTime.Text = FormatTime(TimeSpan.FromSeconds(_playbackManager.Position));
                }
            }
        }

        private void UpdateDuration()
        {
            var timeline = FindName("TimelineSlider") as Slider;
            var totalTime = FindName("TotalTimeText") as TextBlock;
            
            if (timeline != null && totalTime != null)
            {
                timeline.Maximum = _playbackManager.Duration;
                totalTime.Text = FormatTime(TimeSpan.FromSeconds(_playbackManager.Duration));
            }
        }

        private void UpdateCurrentTrackDisplay()
        {
            if (_playbackManager.CurrentTrack != null)
            {
                var trackTitle = FindName("TrackTitleText") as TextBlock;
                var artistText = FindName("ArtistText") as TextBlock;
                
                if (trackTitle != null)
                    trackTitle.Text = _playbackManager.CurrentTrack.Title;
                
                if (artistText != null)
                    artistText.Text = _playbackManager.CurrentTrack.Artist;
            }
        }

        private void LoadAudio()
        {
            try
            {
                audioFileReader = new AudioFileReader(audioFilePath);
                wavePlayer = new WaveOutEvent();
                wavePlayer.Init(audioFileReader);
                wavePlayer.Volume = (float)(VolumeSlider.Value / 100.0);
                wavePlayer.PlaybackStopped += WavePlayer_PlaybackStopped;
                TimelineSlider.Maximum = audioFileReader.TotalTime.TotalSeconds;
                TotalTimeText.Text = FormatTime(audioFileReader.TotalTime);
                LoadMetadata();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadMetadata()
        {
            try
            {
                var file = TagLib.File.Create(audioFilePath);
                if (!string.IsNullOrEmpty(file.Tag.Title))
                {
                    TrackTitleText.Text = file.Tag.Title;
                }
                else
                {
                    TrackTitleText.Text = Path.GetFileNameWithoutExtension(audioFilePath);
                }
                if (file.Tag.Performers != null && file.Tag.Performers.Length > 0)
                {
                    ArtistText.Text = string.Join(", ", file.Tag.Performers);
                }
                else if (file.Tag.AlbumArtists != null && file.Tag.AlbumArtists.Length > 0)
                {
                    ArtistText.Text = string.Join(", ", file.Tag.AlbumArtists);
                }
                else
                {
                    ArtistText.Text = "Unknown Artist";
                }
                if (file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
                {
                    var picture = file.Tag.Pictures[0];
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(picture.Data.Data);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    AlbumArtImage.Source = bitmap;
                    AlbumArtImage.Visibility = Visibility.Visible;
                    AlbumArtPlaceholder.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                TrackTitleText.Text = Path.GetFileNameWithoutExtension(audioFilePath);
                ArtistText.Text = "Unknown Artist";
            }
        }

        private void WavePlayer_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (!suppressPlaybackStopped)
            {
                Dispatcher.Invoke(() =>
                {
                    if (FindName("PlayPauseButton") is Button playPauseButton)
                        playPauseButton.Content = "[ PLAY ]";
                    isPlaying = false;
                });
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_playbackManager.IsPlaying)
            {
                _playbackManager.Pause();
            }
            else
            {
                _playbackManager.Play();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _playbackManager.Stop();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _playbackManager.Volume = e.NewValue / 100.0;
        }

        private void TimelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void TimelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDraggingSlider = false;
            _playbackManager.SeekTo(TimelineSlider.Value);
        }

        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDraggingSlider)
            {
                var currentTime = FindName("CurrentTimeText") as TextBlock;
                if (currentTime != null)
                {
                    currentTime.Text = FormatTime(TimeSpan.FromSeconds(TimelineSlider.Value));
                }
            }
        }

        private void TimelineSlider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            _playbackManager.Previous();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            _playbackManager.Next();
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide miniplayer and show library
            this.Hide();
            
            if (_libraryWindow == null || !_libraryWindow.IsLoaded)
            {
                _libraryWindow = new MusicLibraryWindow();
                _libraryWindow.Closed += (s, args) => 
                {
                    _libraryWindow = null;
                    this.Show();
                };
            }
            
            _libraryWindow.Show();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return time.ToString(@"hh\:mm\:ss");
            }
            return time.ToString(@"mm\:ss");
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SwitchTrack(string newFilePath)
        {
            if (isSwitchingTrack)
            {
                return;
            }
            isSwitchingTrack = true;
            suppressPlaybackStopped = true;
            timer.Stop();
            if (wavePlayer != null)
            {
                wavePlayer.PlaybackStopped -= WavePlayer_PlaybackStopped;
                try { wavePlayer.Stop(); } catch { }
                try { wavePlayer.Dispose(); } catch { }
                wavePlayer = null;
            }
            if (audioFileReader != null)
            {
                try { audioFileReader.Dispose(); } catch { }
                audioFileReader = null;
            }
            TimelineSlider.Value = 0;
            CurrentTimeText.Text = "00:00";
            isPlaying = false;
            audioFilePath = newFilePath;
            LoadAudio();
            UpdatePlaylistButtons();
            if (wavePlayer != null && audioFileReader != null)
            {
                audioFileReader.Position = 0;
                suppressPlaybackStopped = false; 
                wavePlayer.Play();
                timer.Start();
                PlayPauseButton.Content = "[ PAUSE ]";
                isPlaying = true;
            }
            isSwitchingTrack = false;
        }
        private void UpdatePlaylistButtons()
        {
            PreviousButton.IsEnabled = playlist.HasPrevious();
            NextButton.IsEnabled = playlist.HasNext();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer.Stop();
            suppressPlaybackStopped = true;
            if (wavePlayer != null)
            {
                wavePlayer.PlaybackStopped -= WavePlayer_PlaybackStopped;
                try { wavePlayer.Stop(); } catch { }
                try { wavePlayer.Dispose(); } catch { }
            }
            if (audioFileReader != null)
            {
                try { audioFileReader.Dispose(); } catch { }
            }
        }
    }
}