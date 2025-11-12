using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAudio.Wave;
namespace BobMediaPlayer
{
    public partial class AudioPlayerWindow : Window
    {
        private IWavePlayer? wavePlayer;
        private AudioFileReader? audioFileReader;
        private DispatcherTimer timer;
        private bool isPlaying = false;
        private bool isDraggingSlider = false;
        private string audioFilePath;
        private Playlist playlist = new Playlist();
        private bool isSwitchingTrack = false;
        private bool suppressPlaybackStopped = false;
        public AudioPlayerWindow(string filePath)
        {
            InitializeComponent();
            this.Topmost = true;
            audioFilePath = filePath;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            playlist.SetCurrentFile(filePath);
            string? directory = Path.GetDirectoryName(filePath);
            if (directory != null)
            {
                string[] audioExtensions = { ".mp3", ".wav", ".m4a", ".wma" };
                playlist.AddDirectory(directory, audioExtensions);
            }
            LoadAudio();
            UpdatePlaylistButtons();
            MouseLeftButtonDown += (s, e) => DragMove();
            if (wavePlayer != null && audioFileReader != null)
            {
                audioFileReader.Position = 0;
                suppressPlaybackStopped = false;
                wavePlayer.Play();
                timer.Start();
                PlayPauseButton.Content = "[ PAUSE ]";
                isPlaying = true;
            }
        }
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
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
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (wavePlayer == null) return;
            if (isPlaying)
            {
                wavePlayer.Pause();
                timer.Stop();
                PlayPauseButton.Content = "[ PLAY ]";
                isPlaying = false;
            }
            else
            {
                wavePlayer.Play();
                timer.Start();
                PlayPauseButton.Content = "[ PAUSE ]";
                isPlaying = true;
            }
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (wavePlayer == null || audioFileReader == null) return;
            wavePlayer.Stop();
            audioFileReader.Position = 0;
            timer.Stop();
            PlayPauseButton.Content = "[ PLAY ]";
            isPlaying = false;
            TimelineSlider.Value = 0;
            CurrentTimeText.Text = "00:00";
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (wavePlayer != null)
            {
                wavePlayer.Volume = (float)(VolumeSlider.Value / 100.0);
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (audioFileReader != null && !isDraggingSlider)
            {
                TimelineSlider.Value = audioFileReader.CurrentTime.TotalSeconds;
                CurrentTimeText.Text = FormatTime(audioFileReader.CurrentTime);
            }
        }
        private void TimelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDraggingSlider = true;
        }
        private void TimelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDraggingSlider = false;
            if (audioFileReader != null)
            {
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(TimelineSlider.Value);
            }
        }
        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDraggingSlider)
            {
                CurrentTimeText.Text = FormatTime(TimeSpan.FromSeconds(TimelineSlider.Value));
            }
        }
        private void TimelineSlider_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (audioFileReader != null)
            {
                var slider = (Slider)sender;
                var position = e.GetPosition(slider);
                var percentage = position.X / slider.ActualWidth;
                var newValue = percentage * slider.Maximum;
                slider.Value = newValue;
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(newValue);
            }
        }
        private void WavePlayer_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (suppressPlaybackStopped)
            {
                return;
            }
            Dispatcher.Invoke(() =>
            {
                timer.Stop();
                PlayPauseButton.Content = "[ PLAY ]";
                isPlaying = false;
                if (audioFileReader != null && audioFileReader.Position >= audioFileReader.Length)
                {
                    audioFileReader.Position = 0;
                    TimelineSlider.Value = 0;
                    CurrentTimeText.Text = "00:00";
                }
            });
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
        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            string? previousFile = playlist.Previous();
            if (previousFile != null)
            {
                SwitchTrack(previousFile);
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            string? nextFile = playlist.Next();
            if (nextFile != null)
            {
                SwitchTrack(nextFile);
            }
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