    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Microsoft.Win32;
    using System.Windows.Media;

    namespace BobMediaPlayer
    {
        public partial class MainWindow : Window
        {
            private DispatcherTimer timer;
            private bool isPlaying = false;
            private bool isDraggingSlider = false;
            private string? currentMediaPath;
            private MediaType currentMediaType = MediaType.None;
            private AudioPlayerWindow? audioWindow;
            private bool isFullscreen = false;
            private Rect previousWindowBounds;
            private WindowState previousWindowState;
            private ResizeMode previousResizeMode;
            private DispatcherTimer hideControlsTimer;
            private DispatcherTimer hideCursorTimer;
            private Point lastMousePosition;
            private bool controlsVisible = true;
            private double playbackSpeed = 1.0;
            private List<SubtitleEntry> subtitles = new List<SubtitleEntry>();
            private string? currentSubtitlePath;
            private double previousVolume = 50;
            private Playlist playlist = new Playlist();
            private bool autoRotationApplied = false;
            private bool userRotated = false;
            private bool layoutRotateHooked = false;
            private bool preferPortrait270 = true;
            private double currentRotationAngle = 0;
            private AppSettings appSettings = new AppSettings();

            public MainWindow()
            {
                InitializeComponent();
                this.Topmost = true;
                
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(100);
                timer.Tick += Timer_Tick;
                
                hideControlsTimer = new DispatcherTimer();
                hideControlsTimer.Interval = TimeSpan.FromSeconds(3);
                hideControlsTimer.Tick += HideControlsTimer_Tick;
                
                hideCursorTimer = new DispatcherTimer();
                hideCursorTimer.Interval = TimeSpan.FromSeconds(2);
                hideCursorTimer.Tick += HideCursorTimer_Tick;
                this.MouseMove += Window_MouseMove;
                
                ApplySettings();
            }

            private void ApplyImageUIMode()
            {
                TimelinePanel.Visibility = Visibility.Collapsed;
                ControlButtonsPanel.Visibility = Visibility.Visible; 
                OpenButton.Visibility = Visibility.Visible;
                FullscreenButton.Visibility = Visibility.Visible;
                FullscreenButton.IsEnabled = true;

                PlayPauseButton.Visibility = Visibility.Collapsed;
                JumpBackButton.Visibility = Visibility.Collapsed;
                JumpForwardButton.Visibility = Visibility.Collapsed;
                SubtitleButton.Visibility = Visibility.Collapsed;
                SettingsButton.Visibility = Visibility.Visible;
                SettingsButton.IsEnabled = true;

                PreviousButton.Visibility = Visibility.Visible;
                NextButton.Visibility = Visibility.Visible;
                UpdatePlaylistButtons();

                VolumePanel.Visibility = Visibility.Collapsed;
                SeparatorAfterNav.Visibility = Visibility.Collapsed;
                SeparatorBeforeVolume.Visibility = Visibility.Collapsed;

                SubtitlePanel.Visibility = Visibility.Collapsed;
            }

            private void ApplyVideoUIMode()
            {
                TimelinePanel.Visibility = Visibility.Visible;
                ControlButtonsPanel.Visibility = Visibility.Visible;

                OpenButton.Visibility = Visibility.Visible;
                FullscreenButton.Visibility = Visibility.Visible;
                FullscreenButton.IsEnabled = true;

                PlayPauseButton.Visibility = Visibility.Visible;
                PlayPauseButton.IsEnabled = true;
                JumpBackButton.Visibility = Visibility.Visible;
                JumpBackButton.IsEnabled = true;
                JumpForwardButton.Visibility = Visibility.Visible;
                JumpForwardButton.IsEnabled = true;

                SubtitleButton.Visibility = Visibility.Visible;
                SubtitleButton.IsEnabled = true;
                SettingsButton.Visibility = Visibility.Visible;
                SettingsButton.IsEnabled = true;

                PreviousButton.Visibility = Visibility.Visible;
                NextButton.Visibility = Visibility.Visible;

                VolumePanel.Visibility = Visibility.Visible;
                SeparatorAfterNav.Visibility = Visibility.Visible;
                SeparatorBeforeVolume.Visibility = Visibility.Visible;
            }

            private void OpenFile_Click(object sender, RoutedEventArgs e)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Media Files|*.mp3;*.wav;*.m4a;*.wma;*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                            "Audio Files|*.mp3;*.wav;*.m4a;*.wma|" +
                            "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv|" +
                            "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                            "All Files|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    LoadMedia(openFileDialog.FileName);
                }
            }

            public void LoadMediaFromCommandLine(string filePath)
            {

                LoadMedia(filePath);
            }

            private void LoadMedia(string filePath)
            {
                currentMediaPath = filePath;
                string extension = Path.GetExtension(filePath).ToLower();
            
                playlist.Clear(); // Clear existing playlist
                playlist.SetCurrentFile(filePath);
                string? directory = Path.GetDirectoryName(filePath);
                if (directory != null)
                {
                    if (IsVideoFile(extension))
                    {
                        string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv" };
                        playlist.AddDirectory(directory, videoExtensions);
                    }
                    else if (IsImageFile(extension))
                    {
                        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        playlist.AddDirectory(directory, imageExtensions);
                    }
                    else if (IsAudioFile(extension))
                    {
                        string[] audioExtensions = { ".mp3", ".wav", ".m4a", ".wma" };
                        playlist.AddDirectory(directory, audioExtensions);
                    }
                    playlist.SetCurrentFile(filePath);
                }
                
                UpdatePlaylistButtons();
                if (IsAudioFile(extension))
                {
                    LoadAudio(filePath);
                }
                else if (IsVideoFile(extension))
                {
                    WelcomeScreen.Visibility = Visibility.Collapsed;
                    MediaInfoText.Text = Path.GetFileName(filePath);
                    LoadVideo(filePath);
                }
                else if (IsImageFile(extension))
                {
                    WelcomeScreen.Visibility = Visibility.Collapsed;
                    MediaInfoText.Text = Path.GetFileName(filePath);
                    LoadImage(filePath);
                }
            }

            private void LoadAudio(string filePath)
            {
                currentMediaType = MediaType.Audio;
                
    
                if (audioWindow != null)
                {
                    audioWindow.Closed -= AudioWindow_Closed;
                    audioWindow.Close();
                }
                
                audioWindow = new AudioPlayerWindow(filePath);
                audioWindow.Closed += AudioWindow_Closed;
                
                this.Hide();
                audioWindow.Show();
            }

            private void AudioWindow_Closed(object? sender, EventArgs e)
            {
                if (audioWindow != null)
                {
                    audioWindow.Closed -= AudioWindow_Closed;
                    audioWindow = null;
                }
                this.Show();
                WelcomeScreen.Visibility = Visibility.Visible;
            }

            private void LoadVideo(string filePath)
            {
            
                audioWindow?.Close();
                
        
                this.Show();
                
        
                if (VideoPlayer.Source != null)
                {
                    VideoPlayer.Stop();
                    timer.Stop();
                    isPlaying = false;
                }
                

                currentRotationAngle = 0;
                VideoRotateTransform.Angle = 0;
                userRotated = false;
                
                VideoContainer.Visibility = Visibility.Visible;
                ImageContainer.Visibility = Visibility.Collapsed;
                

                TimelineSlider.Value = 0;
                CurrentTimeText.Text = "00:00";
                TotalTimeText.Text = "00:00";
                
                VideoPlayer.Source = new Uri(filePath);
                VideoPlayer.Volume = VolumeSlider.Value / 100.0;
                
                ApplyVideoUIMode();
                
            
                LoadSubtitles(filePath);
                
                currentMediaType = MediaType.Video;
                autoRotationApplied = false;
                
                MediaDisplayGrid.SizeChanged -= MediaDisplayGrid_SizeChangedForAutoRotate;
                MediaDisplayGrid.SizeChanged += MediaDisplayGrid_SizeChangedForAutoRotate;

                if (!layoutRotateHooked)
                {
                    layoutRotateHooked = true;
                    EventHandler handler = null!;
                    handler = (s, e2) =>
                    {
                        if (!userRotated && !autoRotationApplied &&
                            MediaDisplayGrid.ActualWidth > 0 && MediaDisplayGrid.ActualHeight > 0 &&
                            VideoPlayer.NaturalVideoWidth > 0 && VideoPlayer.NaturalVideoHeight > 0)
                        {
                            AutoRotateVideo();
                            autoRotationApplied = true;
                            VideoPlayer.LayoutUpdated -= handler;
                            layoutRotateHooked = false;
                        }
                    };
                    VideoPlayer.LayoutUpdated += handler;
                }
                VideoPlayer.Play();
                timer.Start();
                PlayPauseButton.Content = "[ PAUSE ]";
                isPlaying = true;
            }

            private void LoadImage(string filePath)
            {
                this.Show();
                
                currentRotationAngle = 0;
                ImageRotateTransform.Angle = 0;
                
                VideoContainer.Visibility = Visibility.Collapsed;
                ImageContainer.Visibility = Visibility.Visible;
                
                if (ImageViewer.Source != null)
                {
                    (ImageViewer.Source as BitmapImage)?.StreamSource?.Dispose();
                    ImageViewer.Source = null;
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                
                ImageViewer.Source = bitmap;
                
                TimelineSlider.IsEnabled = false;
                CurrentTimeText.Text = "00:00";
                TotalTimeText.Text = "00:00";
                ApplyImageUIMode();
                
                currentMediaType = MediaType.Image;
            }

            private void PlayPause_Click(object sender, RoutedEventArgs e)
            {
                if (currentMediaType == MediaType.Video)
                {
                    if (isPlaying)
                    {
                        VideoPlayer.Pause();
                        timer.Stop();
                        PlayPauseButton.Content = "[ PLAY ]";
                        isPlaying = false;
                    }
                    else
                    {
                        VideoPlayer.Play();
                        timer.Start();
                        PlayPauseButton.Content = "[ PAUSE ]";
                        isPlaying = true;
                    }
                }
            }

            private void Stop_Click(object? sender, RoutedEventArgs? e)
            {
                if (currentMediaType == MediaType.Video)
                {
                    VideoPlayer.Stop();
                    timer.Stop();
                    PlayPauseButton.Content = "[ PLAY ]";
                    isPlaying = false;
                    TimelineSlider.Value = 0;
                    CurrentTimeText.Text = "00:00";
                }
            }

            private void Previous_Click(object sender, RoutedEventArgs e)
            {
                string? previousFile = playlist.Previous();
                if (previousFile != null)
                {
                
                    if (currentMediaType == MediaType.Video)
                    {
                        VideoPlayer.Stop();
                        timer.Stop();
                        isPlaying = false;
                    }
                    
                    LoadMedia(previousFile);
                }
            }

            private void Next_Click(object sender, RoutedEventArgs e)
            {
                string? nextFile = playlist.Next();
                if (nextFile != null)
                {

                    if (currentMediaType == MediaType.Video)
                    {
                        VideoPlayer.Stop();
                        timer.Stop();
                        isPlaying = false;
                    }
                    
                    LoadMedia(nextFile);
                }
            }
            
            private void UpdatePlaylistButtons()
            {
                PreviousButton.IsEnabled = playlist.HasPrevious();
                NextButton.IsEnabled = playlist.HasNext();
            }

            private void JumpBack_Click(object sender, RoutedEventArgs e)
            {
                JumpBackward();
            }

            private void JumpForward_Click(object sender, RoutedEventArgs e)
            {
                JumpForward();
            }

            private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                if (VideoPlayer != null)
                {
                    VideoPlayer.Volume = VolumeSlider.Value / 100.0;
                }
            }

            private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
            {
                if (VideoPlayer.NaturalDuration.HasTimeSpan)
                {
                    TimelineSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    TotalTimeText.Text = FormatTime(VideoPlayer.NaturalDuration.TimeSpan);
                    TimelineSlider.IsEnabled = true;
                }
                AutoRotateVideo();
                
                
            }
            
            private void AutoRotateVideo()
            {
                if (userRotated) return;
                double w = VideoPlayer.NaturalVideoWidth;
                double h = VideoPlayer.NaturalVideoHeight;
                if (w <= 0 || h <= 0 || MediaDisplayGrid.ActualWidth <= 0 || MediaDisplayGrid.ActualHeight <= 0)
                {
                    currentRotationAngle = 0;
                    VideoRotateTransform.Angle = 0;
                    return;
                }

                double cw = MediaDisplayGrid.ActualWidth;
                double ch = MediaDisplayGrid.ActualHeight;

                double aspect = h / w; 
                if (aspect <= 1.05) 
                {
                    currentRotationAngle = 0;
                    VideoRotateTransform.Angle = 0;
                    return;
                }

                double BestAreaFor(int angle, out double rwOut, out double rhOut)
                {
                    bool swap = (angle == 90 || angle == 270);
                    double rw = swap ? h : w;
                    double rh = swap ? w : h;
                    double scale = Math.Min(cw / rw, ch / rh);
                    rwOut = rw * scale;
                    rhOut = rh * scale;
                    return (rw * rh) * scale * scale;
                }

                int[] portraitOrder = preferPortrait270 ? new[] { 270, 90 } : new[] { 90, 270 };
                int chosen = 90; 
                double bestAreaPortrait = -1;
                foreach (var a in portraitOrder)
                {
                    double rwDisp, rhDisp;
                    double area = BestAreaFor(a, out rwDisp, out rhDisp);
                    bool isPortrait = rwDisp <= rhDisp;
                    if (isPortrait && area > bestAreaPortrait)
                    {
                        bestAreaPortrait = area;
                        chosen = a;
                    }
                }
                if (bestAreaPortrait < 0)
                {
                    chosen = 0;
                }
                currentRotationAngle = chosen;
                VideoRotateTransform.Angle = chosen;
                if (chosen != 0)
                {
                    ShowOSD($"Auto-rotated {chosen}° (portrait)");
                }
            }

            private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
            {
                Stop_Click(null, null);
            }

            private void MediaDisplayGrid_SizeChangedForAutoRotate(object sender, SizeChangedEventArgs e)
            {
                if (currentMediaType == MediaType.Video && !autoRotationApplied && !userRotated)
                {
                    if (MediaDisplayGrid.ActualWidth > 0 && MediaDisplayGrid.ActualHeight > 0 &&
                        VideoPlayer.NaturalVideoWidth > 0 && VideoPlayer.NaturalVideoHeight > 0)
                    {
                        AutoRotateVideo();
                        autoRotationApplied = true;
                        MediaDisplayGrid.SizeChanged -= MediaDisplayGrid_SizeChangedForAutoRotate;
                    }
                }
                else if (userRotated)
                {
                    MediaDisplayGrid.SizeChanged -= MediaDisplayGrid_SizeChangedForAutoRotate;
                    autoRotationApplied = true;
                }
            }

            private void Timer_Tick(object? sender, EventArgs e)
            {
                if (!isDraggingSlider && VideoPlayer.Source != null)
                {
                    TimelineSlider.Value = VideoPlayer.Position.TotalSeconds;
                    CurrentTimeText.Text = FormatTime(VideoPlayer.Position);
                    
                    UpdateSubtitles();
                }
            }

            private void TimelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
            {
                isDraggingSlider = true;
            }

            private void TimelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
            {
                isDraggingSlider = false;
                if (currentMediaType == MediaType.Video)
                {
                    VideoPlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
                }
            }

            private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                if (isDraggingSlider && currentMediaType == MediaType.Video)
                {
                    CurrentTimeText.Text = FormatTime(TimeSpan.FromSeconds(TimelineSlider.Value));
                }
            }

            private void TimelineSlider_MouseDown(object sender, MouseButtonEventArgs e)
            {
                if (currentMediaType == MediaType.Video && e.LeftButton == MouseButtonState.Pressed)
                {
                    var slider = sender as System.Windows.Controls.Slider;
                    if (slider != null)
                    {
                        Point position = e.GetPosition(slider);
                        double percentage = position.X / slider.ActualWidth;
                        double newValue = percentage * slider.Maximum;
                        slider.Value = newValue;
                        VideoPlayer.Position = TimeSpan.FromSeconds(newValue);
                    }
                }
            }

            private string FormatTime(TimeSpan time)
            {
                if (time.TotalHours >= 1)
                {
                    return time.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    return time.ToString(@"mm\:ss");
                }
            }

            private void Window_Drop(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0)
                    {
                        LoadMedia(files[0]);
                    }
                }
            }

            private bool IsAudioFile(string extension)
            {
                return extension == ".mp3" || extension == ".wav" || 
                    extension == ".m4a" || extension == ".wma";
            }

            private bool IsVideoFile(string extension)
            {
                return extension == ".mp4" || extension == ".avi" || extension == ".mkv" || 
                    extension == ".mov" || extension == ".wmv";
            }

            private bool IsImageFile(string extension)
            {
                return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || 
                    extension == ".gif" || extension == ".bmp";
            }

            private void Fullscreen_Click(object sender, RoutedEventArgs e)
            {
                ToggleFullscreen();
            }

            private void Media_DoubleClick(object sender, MouseButtonEventArgs e)
            {
                if (e.ClickCount == 2)
                {
                    ToggleFullscreen();
                }
            }

            private void Window_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.F11)
                {
                    ToggleFullscreen();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape && isFullscreen)
                {
                    ExitFullscreen();
                    e.Handled = true;
                }
                else if (e.Key == Key.Space && (currentMediaType == MediaType.Video || currentMediaType == MediaType.Image))
                {
                    if (!(Keyboard.FocusedElement is System.Windows.Controls.Button))
                    {
                        if (currentMediaType == MediaType.Video)
                        {
                            PlayPause_Click(sender, new RoutedEventArgs());
                        }
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Right && currentMediaType == MediaType.Video)
                {
                    JumpForward();
                    e.Handled = true;
                }
                else if (e.Key == Key.Left && currentMediaType == MediaType.Video)
                {
                    JumpBackward();
                    e.Handled = true;
                }
                else if (e.Key == Key.Right && currentMediaType == MediaType.Image)
                {
                    Next_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == Key.Left && currentMediaType == MediaType.Image)
                {
                    Previous_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == Key.Up && currentMediaType == MediaType.Video)
                {
                    VolumeSlider.Value = Math.Min(100, VolumeSlider.Value + 5);
                    ShowOSD($"Volume: {(int)VolumeSlider.Value}%");
                    e.Handled = true;
                }
                else if (e.Key == Key.Down && currentMediaType == MediaType.Video)
                {
                    VolumeSlider.Value = Math.Max(0, VolumeSlider.Value - 5);
                    ShowOSD($"Volume: {(int)VolumeSlider.Value}%");
                    e.Handled = true;
                }
                else if (e.Key == Key.M)
                {
                    ToggleMute();
                    e.Handled = true;
                }
                else if (e.Key == Key.OemPlus || e.Key == Key.Add)
                {
                    IncreaseSpeed();
                    e.Handled = true;
                }
                else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
                {
                    DecreaseSpeed();
                    e.Handled = true;
                }
            }

            private void ToggleFullscreen()
            {
                if (isFullscreen)
                {
                    ExitFullscreen();
                }
                else
                {
                    EnterFullscreen();
                }
            }

            private void EnterFullscreen()
            {
                if (currentMediaType == MediaType.Video || currentMediaType == MediaType.Image)
                {
                    previousWindowBounds = new Rect(Left, Top, Width, Height);
                    previousWindowState = WindowState;
                    previousResizeMode = ResizeMode;

                    var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                    var screenBounds = screen.Bounds;

                    Left = screenBounds.X;
                    Top = screenBounds.Y;
                    Width = screenBounds.Width;
                    Height = screenBounds.Height;

                    ResizeMode = ResizeMode.NoResize;
                    WindowState = WindowState.Normal; 

                    ControlPanel.Visibility = Visibility.Collapsed;
                    TitleBar.Visibility = Visibility.Collapsed;
                    SettingsBar.Visibility = Visibility.Collapsed;
                    controlsVisible = false;

                    isFullscreen = true;

                    hideControlsTimer.Start();
                    hideCursorTimer.Start();
                }
            }

            private void ExitFullscreen()
            {
                if (isFullscreen)
                {
                    Left = previousWindowBounds.Left;
                    Top = previousWindowBounds.Top;
                    Width = previousWindowBounds.Width;
                    Height = previousWindowBounds.Height;
                    WindowState = previousWindowState;
                    ResizeMode = previousResizeMode;

                    ControlPanel.Visibility = Visibility.Visible;
                    TitleBar.Visibility = Visibility.Visible;
                    controlsVisible = true;

                    isFullscreen = false;

                    hideControlsTimer.Stop();
                    hideCursorTimer.Stop();

                    this.Cursor = Cursors.Arrow;
                }
            }

            private void Window_MouseMove(object sender, MouseEventArgs e)
            {
                if (isFullscreen)
                {
                    Point currentPosition = e.GetPosition(this);
                    
                    if (currentPosition != lastMousePosition)
                    {
                        lastMousePosition = currentPosition;
                        
                        this.Cursor = Cursors.Arrow;
                        hideCursorTimer.Stop();
                        hideCursorTimer.Start();
                        
                        if (currentPosition.Y > this.ActualHeight - 150)
                        {
                            ShowControls();
                            hideControlsTimer.Stop();
                            hideControlsTimer.Start();
                        }
                        else
                        {
                            hideControlsTimer.Stop();
                            hideControlsTimer.Start();
                        }
                    }
                }
            }

            private void HideControlsTimer_Tick(object? sender, EventArgs e)
            {
                if (isFullscreen && controlsVisible)
                {
                    HideControls();
                }
            }

            private void HideCursorTimer_Tick(object? sender, EventArgs e)
            {
                if (isFullscreen)
                {
                    this.Cursor = Cursors.None;
                }
            }

            private void ShowControls()
            {
                if (!controlsVisible && isFullscreen)
                {
                    ControlPanel.Visibility = Visibility.Visible;
                    controlsVisible = true;
                }
            }

            private void HideControls()
            {
                if (controlsVisible && isFullscreen)
                {
                    ControlPanel.Visibility = Visibility.Collapsed;
                    controlsVisible = false;
                }
            }

            private void JumpForward()
            {
                if (currentMediaType == MediaType.Video && VideoPlayer.Source != null)
                {
                    var newPosition = VideoPlayer.Position.Add(TimeSpan.FromSeconds(10));
                    if (newPosition < VideoPlayer.NaturalDuration.TimeSpan)
                    {
                        VideoPlayer.Position = newPosition;
                        ShowOSD("+10s");
                    }
                }
            }

            private void JumpBackward()
            {
                if (currentMediaType == MediaType.Video && VideoPlayer.Source != null)
                {
                    var newPosition = VideoPlayer.Position.Subtract(TimeSpan.FromSeconds(10));
                    if (newPosition < TimeSpan.Zero)
                        newPosition = TimeSpan.Zero;
                    VideoPlayer.Position = newPosition;
                    ShowOSD("-10s");
                }
            }

            private void ToggleMute()
            {
                if (VolumeSlider.Value > 0)
                {
                    previousVolume = VolumeSlider.Value;
                    VolumeSlider.Value = 0;
                    ShowOSD("Muted");
                }
                else
                {
                    VolumeSlider.Value = previousVolume;
                    ShowOSD($"Volume: {(int)previousVolume}%");
                }
            }

            private void IncreaseSpeed()
            {
                if (currentMediaType == MediaType.Video)
                {
                    playbackSpeed = Math.Min(2.0, playbackSpeed + 0.25);
                    VideoPlayer.SpeedRatio = playbackSpeed;
                    ShowOSD($"Speed: {playbackSpeed:F2}x");
                }
            }

            private void DecreaseSpeed()
            {
                if (currentMediaType == MediaType.Video)
                {
                    playbackSpeed = Math.Max(0.25, playbackSpeed - 0.25);
                    VideoPlayer.SpeedRatio = playbackSpeed;
                    ShowOSD($"Speed: {playbackSpeed:F2}x");
                }
            }

            private DispatcherTimer? osdTimer;
            private void ShowOSD(string message)
            {
                if (OSDText != null)
                {
                    OSDText.Text = message;
                    OSDPanel.Visibility = Visibility.Visible;
                    
                    osdTimer?.Stop();
                    osdTimer = new DispatcherTimer();
                    osdTimer.Interval = TimeSpan.FromSeconds(1.5);
                    osdTimer.Tick += (s, e) =>
                    {
                        OSDPanel.Visibility = Visibility.Collapsed;
                        osdTimer.Stop();
                    };
                    osdTimer.Start();
                }
            }

            private void LoadSubtitles(string videoPath)
            {
                string srtPath = Path.ChangeExtension(videoPath, ".srt");
                if (File.Exists(srtPath))
                {
                    currentSubtitlePath = srtPath;
                    subtitles = ParseSRT(srtPath);
                    if (subtitles.Count > 0)
                    {
                        ShowOSD($"Subtitles loaded: {subtitles.Count} entries");
                    }
                }
            }

            private List<SubtitleEntry> ParseSRT(string filePath)
            {
                var entries = new List<SubtitleEntry>();
                try
                {
                    var lines = File.ReadAllLines(filePath);
                    SubtitleEntry? currentEntry = null;
                    var textLines = new List<string>();

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (currentEntry != null && textLines.Count > 0)
                            {
                                currentEntry.Text = string.Join("\n", textLines);
                                entries.Add(currentEntry);
                                currentEntry = null;
                                textLines.Clear();
                            }
                        }
                        else if (int.TryParse(line.Trim(), out _))
                        {
                            currentEntry = new SubtitleEntry();
                        }
                        else if (line.Contains("-->"))
                        {
                            var times = line.Split(new[] { "-->" }, StringSplitOptions.None);
                            if (times.Length == 2 && currentEntry != null)
                            {
                                currentEntry.StartTime = ParseSRTTime(times[0].Trim());
                                currentEntry.EndTime = ParseSRTTime(times[1].Trim());
                            }
                        }
                        else if (currentEntry != null)
                        {
                            textLines.Add(line.Trim());
                        }
                    }
                    if (currentEntry != null && textLines.Count > 0)
                    {
                        currentEntry.Text = string.Join("\n", textLines);
                        entries.Add(currentEntry);
                    }
                }
                catch (Exception ex)
                {
                    ShowOSD($"Error loading subtitles: {ex.Message}");
                }
                return entries;
            }

            private TimeSpan ParseSRTTime(string timeString)
            {
                timeString = timeString.Replace(',', '.');
                if (TimeSpan.TryParse(timeString, out TimeSpan result))
                {
                    return result;
                }
                return TimeSpan.Zero;
            }

            private void UpdateSubtitles()
            {
                if (subtitles.Count == 0 || currentMediaType != MediaType.Video)
                {
                    SubtitlePanel.Visibility = Visibility.Collapsed;
                    return;
                }

                var currentTime = VideoPlayer.Position;
                var currentSubtitle = subtitles.FirstOrDefault(s => 
                    currentTime >= s.StartTime && currentTime <= s.EndTime);

                if (currentSubtitle != null)
                {
                    SubtitleText.Text = currentSubtitle.Text;
                    SubtitlePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    SubtitlePanel.Visibility = Visibility.Collapsed;
                }
            }

            private void Minimize_Click(object sender, RoutedEventArgs e)
            {
                WindowState = WindowState.Minimized;
            }

            private void Close_Click(object sender, RoutedEventArgs e)
            {
                Close();
            }

            private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (e.ClickCount == 1)
                {
                    DragMove();
                }
            }

            private void Subtitle_Click(object sender, RoutedEventArgs e)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Subtitle Files|*.srt|All Files|*.*",
                    Title = "Select Subtitle File"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    subtitles = ParseSRT(openFileDialog.FileName);
                    if (subtitles.Count > 0)
                    {
                        ShowOSD($"Subtitles loaded: {subtitles.Count} entries");
                    }
                    else
                    {
                        ShowOSD("No subtitles found in file");
                    }
                }
            }

            private void ToggleSettings_Click(object sender, RoutedEventArgs e)
            {
                var settingsWindow = new SettingsWindow(appSettings);
                settingsWindow.Owner = this;
                if (settingsWindow.ShowDialog() == true)
                {
                    appSettings = settingsWindow.Settings;
                    ApplySettings();
                }
            }

            private void SpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
                {
                    playbackSpeed = Convert.ToDouble(item.Tag);
                    if (currentMediaType == MediaType.Video && VideoPlayer != null)
                    {
                        VideoPlayer.SpeedRatio = playbackSpeed;
                        ShowOSD($"Speed: {playbackSpeed:F2}x");
                    }
                }
            }

            private void SubtitleSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
                {
                    int fontSize = Convert.ToInt32(item.Tag);
                    if (SubtitleText != null)
                    {
                        SubtitleText.FontSize = fontSize;
                        ShowOSD($"Subtitle size: {fontSize}px");
                    }
                }
            }
            
            private void RotateLeft_Click(object sender, RoutedEventArgs e)
            {
                RotateMedia(-90);
            }
            
            private void RotateRight_Click(object sender, RoutedEventArgs e)
            {
                RotateMedia(90);
            }
            
            private void RotateReset_Click(object sender, RoutedEventArgs e)
            {
                currentRotationAngle = 0;
                if (currentMediaType == MediaType.Video)
                {
                    VideoRotateTransform.Angle = 0;
                }
                else if (currentMediaType == MediaType.Image)
                {
                    ImageRotateTransform.Angle = 0;
                }
                ShowOSD("Rotation reset");
                userRotated = true; 
            }
            
            private void RotateMedia(double degrees)
            {
                currentRotationAngle = (currentRotationAngle + degrees) % 360;
                if (currentRotationAngle < 0)
                    currentRotationAngle += 360;
                
                if (currentMediaType == MediaType.Video)
                {
                    VideoRotateTransform.Angle = currentRotationAngle;
                    ShowOSD($"Rotated: {currentRotationAngle}°");
                }
                else if (currentMediaType == MediaType.Image)
                {
                    ImageRotateTransform.Angle = currentRotationAngle;
                    ShowOSD($"Rotated: {currentRotationAngle}°");
                }
                userRotated = true;
                MediaDisplayGrid.SizeChanged -= MediaDisplayGrid_SizeChangedForAutoRotate;
                autoRotationApplied = true;

                if (currentMediaType == MediaType.Video && (currentRotationAngle == 90 || currentRotationAngle == 270))
                {
                    preferPortrait270 = (currentRotationAngle == 270);
                }
            }

            private void ApplySettings()
            {
                this.Topmost = appSettings.AlwaysOnTop;
                VolumeSlider.Value = appSettings.DefaultVolume;
                playbackSpeed = appSettings.DefaultSpeed;
            }

            protected override void OnClosed(EventArgs e)
            {
                base.OnClosed(e);
                hideControlsTimer?.Stop();
                hideCursorTimer?.Stop();
                osdTimer?.Stop();
                audioWindow?.Close();
            }

            private void MediaContainer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
            {
                
                ContextMenu contextMenu = new ContextMenu();
                contextMenu.Background = (Brush)Application.Current.Resources["DarkSecondary"];
                contextMenu.Foreground = (Brush)Application.Current.Resources["TextPrimary"];
                contextMenu.FontFamily = new FontFamily("Consolas");
                
                Style menuItemStyle = (Style)Application.Current.Resources["NoFocusMenuItemStyle"];

                MenuItem rotateLeft = new MenuItem { Header = "Rotate Left 90°", Style = menuItemStyle };
                rotateLeft.Click += (s, args) => RotateMedia(-90);
                contextMenu.Items.Add(rotateLeft);
                

                MenuItem rotateRight = new MenuItem { Header = "Rotate Right 90°", Style = menuItemStyle };
                rotateRight.Click += (s, args) => RotateMedia(90);
                contextMenu.Items.Add(rotateRight);
                

                MenuItem resetRotation = new MenuItem { Header = "Reset Rotation", Style = menuItemStyle };
                resetRotation.Click += (s, args) => RotateReset_Click(this, args);
                contextMenu.Items.Add(resetRotation);
                

                contextMenu.IsOpen = true;
                e.Handled = true;
                
            }
        }

        public enum MediaType
        {
            None,
            Audio,
            Video,
            Image
        }

        public class SubtitleEntry
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public string Text { get; set; } = string.Empty;
        }
    }
