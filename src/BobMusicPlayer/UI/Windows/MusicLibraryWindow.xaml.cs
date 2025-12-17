using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using NAudio.Wave;
using BobMediaPlayer.Library;
using System.ComponentModel;

namespace BobMediaPlayer
{
    public partial class MusicLibraryWindow : Window, INotifyPropertyChanged
    {
        private readonly AudioPlaybackManager _playbackManager = AudioPlaybackManager.Instance;
        private AudioPlayerWindow? _miniPlayerWindow;
        private class ArtistListItem
        {
            public string Name { get; set; } = string.Empty;
            public int TrackCount { get; set; }
            public string Initial => string.IsNullOrWhiteSpace(Name) ? "#" : Name.Substring(0, 1).ToUpperInvariant();
        }

        // TopTracksList functionality moved to AlbumsPanel in new layout

        // Helper method to access UI elements when XAML field generation is broken
        private T GetUIElement<T>(string name) where T : class
        {
            try
            {
                return FindName(name) as T;
            }
            catch
            {
                return null;
            }
        }

        private void UpdateQueueDisplay()
        {
            System.Diagnostics.Debug.WriteLine("UpdateQueueDisplay: Starting");
            System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Current playQueue order - [{string.Join(", ", playQueue)}]");
            System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: CurrentIndex={currentIndex}");
            
            var queuePanel = GetUIElement<System.Windows.Controls.StackPanel>("QueuePanel");
            if (queuePanel != null)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: QueuePanel found, clearing {queuePanel.Children.Count} children");
                queuePanel.Children.Clear();
                
                // Get current album tracks if we have a current album
                var currentAlbumTracks = new List<AudioLibraryTrack>();
                if (!string.IsNullOrEmpty(currentAlbum) && currentIndex >= 0 && currentIndex < playQueue.Count)
                {
                    var currentTrackPath = playQueue[currentIndex];
                    var currentTrack = _all.FirstOrDefault(t => string.Equals(t.Path, currentTrackPath, StringComparison.OrdinalIgnoreCase));
                    
                    if (currentTrack != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Current album is '{currentAlbum}'");
                        // Get all tracks from the same album
                        currentAlbumTracks = _all
                            .Where(t => string.Equals(t.Album, currentAlbum, StringComparison.OrdinalIgnoreCase))
                            .OrderBy(t => t.Title) // You might want to order by track number instead
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Found {currentAlbumTracks.Count} tracks in current album");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: No current album or invalid index. Album: '{currentAlbum}', Index: {currentIndex}, QueueCount: {playQueue.Count}");
                }
                
                // First, add the current playing track at the top
                if (currentIndex >= 0 && currentIndex < playQueue.Count)
                {
                    var currentTrackPath = playQueue[currentIndex];
                    var currentTrack = _all.FirstOrDefault(t => string.Equals(t.Path, currentTrackPath, StringComparison.OrdinalIgnoreCase));
                    
                    if (currentTrack != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Adding current track '{currentTrack.Title}' at top");
                        var queueItem = CreateQueueItem(currentTrack, currentIndex, true);
                        queuePanel.Children.Add(queueItem);
                    }
                }
                
                // Then add the remaining tracks from the current album that come after the current track
                var currentTrackInAlbum = currentAlbumTracks.FirstOrDefault(t => string.Equals(t.Path, playQueue[currentIndex], StringComparison.OrdinalIgnoreCase));
                if (currentTrackInAlbum != null)
                {
                    var currentTrackIndexInAlbum = currentAlbumTracks.IndexOf(currentTrackInAlbum);
                    System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Current track '{currentTrackInAlbum.Title}' is at album index {currentTrackIndexInAlbum}");
                    
                    // Get upcoming album tracks (tracks that come after current in album order)
                    var upcomingAlbumTrackPaths = currentAlbumTracks.Skip(currentTrackIndexInAlbum + 1).Select(t => t.Path).ToHashSet();
                    System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Found {upcomingAlbumTrackPaths.Count} upcoming tracks in album");
                    
                    // Display upcoming tracks in their actual playQueue order (after reordering)
                    for (int queueIndex = 0; queueIndex < playQueue.Count; queueIndex++)
                    {
                        if (queueIndex == currentIndex) continue; // Skip current track (already added)
                        
                        var trackPath = playQueue[queueIndex];
                        if (upcomingAlbumTrackPaths.Contains(trackPath)) // Only if it's an upcoming track from this album
                        {
                            var track = _all.FirstOrDefault(t => string.Equals(t.Path, trackPath, StringComparison.OrdinalIgnoreCase));
                            if (track != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Adding upcoming album track '{track.Title}' at queue position {queueIndex}");
                                var queueItem = CreateQueueItem(track, queueIndex, false);
                                queuePanel.Children.Add(queueItem);
                            }
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"UpdateQueueDisplay: Completed with {queuePanel.Children.Count} items in queue");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UpdateQueueDisplay: QueuePanel is null!");
            }
        }
        
        private Border CreateQueueItem(AudioLibraryTrack track, int playQueueIndex, bool isCurrentTrack)
        {
            var queueItem = new Border
            {
                Background = isCurrentTrack ? 
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 150, 200)) : // Highlight current track in blue
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 70, 80)),
                Margin = new Thickness(8, 2, 8, 2),
                Padding = new Thickness(8, 4, 8, 4),
                Cursor = isCurrentTrack ? System.Windows.Input.Cursors.Arrow : System.Windows.Input.Cursors.Hand,
                Tag = playQueueIndex // Store the actual playQueue index for drag/drop
            };
            
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = track.Title ?? "Unknown Title", 
                FontSize = 11, 
                FontWeight = isCurrentTrack ? FontWeights.Bold : FontWeights.Normal,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 239)) 
            });
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = track.Artist ?? "Unknown Artist", 
                FontSize = 10, 
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 185, 190)) 
            });
            
            queueItem.Child = stackPanel;
            
            // Add drag and drop support - only for non-playing tracks
            if (!isCurrentTrack)
            {
                bool isDragging = false;
                Point startPoint = new Point();
                
                queueItem.MouseLeftButtonDown += (s, e) => {
                    try
                    {
                        startPoint = e.GetPosition(queueItem);
                        isDragging = false;
                        System.Diagnostics.Debug.WriteLine($"MouseLeftButtonDown: playQueueIndex={playQueueIndex}, startPoint={startPoint}");
                        
                        queueItem.CaptureMouse();
                        queueItem.Opacity = 0.8;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"MouseLeftButtonDown error: {ex.Message}");
                    }
                };
                
                queueItem.MouseMove += (s, e) => {
                    try
                    {
                        if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
                        {
                            Point currentPoint = e.GetPosition(queueItem);
                            Vector diff = startPoint - currentPoint;
                            
                            // Start drag if moved enough pixels
                            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                            {
                                isDragging = true;
                                System.Diagnostics.Debug.WriteLine($"Drag start: playQueueIndex={playQueueIndex}, IsCurrentTrack={isCurrentTrack}");
                                
                                // Add drag visual feedback
                                queueItem.Opacity = 0.7;
                                queueItem.Effect = new System.Windows.Media.Effects.DropShadowEffect
                                {
                                    Color = System.Windows.Media.Colors.Black,
                                    Direction = 315,
                                    ShadowDepth = 5,
                                    BlurRadius = 10,
                                    Opacity = 0.5
                                };
                                
                                var data = new DataObject();
                                data.SetData("QueueIndex", playQueueIndex);
                                System.Diagnostics.Debug.WriteLine($"Drag start: Starting DoDragDrop with QueueIndex={playQueueIndex}");
                                var result = DragDrop.DoDragDrop(queueItem, data, DragDropEffects.Move);
                                System.Diagnostics.Debug.WriteLine($"Drag start: DoDragDrop completed with result={result}");
                                
                                // Reset visual effects after drag
                                queueItem.Opacity = 1.0;
                                queueItem.Effect = null;
                                queueItem.ReleaseMouseCapture();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Drag start error: {ex.Message}");
                        queueItem.Opacity = 1.0;
                        queueItem.Effect = null;
                        queueItem.ReleaseMouseCapture();
                    }
                };
                
                queueItem.MouseLeftButtonUp += (s, e) => {
                    try
                    {
                        if (!isDragging)
                        {
                            queueItem.Opacity = 1.0;
                        }
                        queueItem.ReleaseMouseCapture();
                        isDragging = false;
                        System.Diagnostics.Debug.WriteLine($"MouseLeftButtonUp: Released capture");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"MouseLeftButtonUp error: {ex.Message}");
                    }
                };
                
                // Simplified drop handling - just allow drop
                queueItem.AllowDrop = true;
            }
            
            return queueItem;
        }

        private void QueuePanel_DragEnter(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("QueuePanel_DragEnter triggered");
            if (e.Data.GetDataPresent("QueueIndex") && e.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                e.Effects = DragDropEffects.Move;
                System.Diagnostics.Debug.WriteLine("QueuePanel_DragEnter: Allowing move");
            }
            else
            {
                e.Effects = DragDropEffects.None;
                System.Diagnostics.Debug.WriteLine("QueuePanel_DragEnter: Not allowing move");
            }
            e.Handled = true;
        }

        private void QueuePanel_DragLeave(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("QueuePanel_DragLeave triggered");
        }

        private void QueuePanel_Drop(object sender, DragEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("QueuePanel_Drop triggered");
                
                if (e.Data.GetDataPresent("QueueIndex"))
                {
                    var sourceIndex = (int)e.Data.GetData("QueueIndex");
                    System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: SourceIndex={sourceIndex}");
                    
                    // Get the position relative to the queue panel
                    var queuePanel = GetUIElement<System.Windows.Controls.StackPanel>("QueuePanel");
                    if (queuePanel == null)
                    {
                        System.Diagnostics.Debug.WriteLine("QueuePanel_Drop: QueuePanel is null");
                        return;
                    }
                    
                    Point position = e.GetPosition(queuePanel);
                    
                    // Find which queue item is at this position
                    var targetElement = queuePanel.InputHitTest(position) as FrameworkElement;
                    Border targetQueueItem = null;
                    
                    // Traverse up to find the Border (queue item)
                    var element = targetElement;
                    while (element != null && targetQueueItem == null)
                    {
                        if (element is Border border && border.Tag is int)
                        {
                            targetQueueItem = border;
                        }
                        element = element.Parent as FrameworkElement;
                    }
                    
                    if (targetQueueItem != null)
                    {
                        var targetIndex = (int)targetQueueItem.Tag;
                        System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: TargetIndex={targetIndex}");
                        
                        if (sourceIndex != targetIndex && sourceIndex >= 0 && targetIndex >= 0 && 
                            sourceIndex < playQueue.Count && targetIndex < playQueue.Count)
                        {
                            // Don't allow moving the currently playing track
                            if (sourceIndex == currentIndex || targetIndex == currentIndex)
                            {
                                System.Diagnostics.Debug.WriteLine("QueuePanel_Drop: Cannot move current track, operation cancelled");
                                return;
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Moving track from index {sourceIndex} to {targetIndex}");
                            System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Queue before reorder - [{string.Join(", ", playQueue)}]");
                            
                            // Reorder the play queue
                            var sourceTrack = playQueue[sourceIndex];
                            playQueue.RemoveAt(sourceIndex);
                            
                            // Adjust target index if it was affected by removal
                            if (targetIndex > sourceIndex)
                            {
                                targetIndex--;
                                System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Target index adjusted to {targetIndex}");
                            }
                            
                            playQueue.Insert(targetIndex, sourceTrack);
                            
                            System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Queue after reorder - [{string.Join(", ", playQueue)}]");
                            
                            // Update current index if needed (only if it was affected)
                            if (sourceIndex < currentIndex && targetIndex >= currentIndex)
                            {
                                currentIndex--;
                                System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Current index decreased to {currentIndex}");
                            }
                            else if (sourceIndex > currentIndex && targetIndex <= currentIndex)
                            {
                                currentIndex++;
                                System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Current index increased to {currentIndex}");
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Final currentIndex={currentIndex}");
                            System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Queue reordered, updating display");
                            
                            // Force UI refresh
                            Dispatcher.BeginInvoke(() => {
                                var refreshQueuePanel = GetUIElement<System.Windows.Controls.StackPanel>("QueuePanel");
                                if (refreshQueuePanel != null)
                                {
                                    UpdateQueueDisplay();
                                    // Force layout update
                                    refreshQueuePanel.UpdateLayout();
                                    System.Diagnostics.Debug.WriteLine("QueuePanel_Drop: UI refresh completed");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("QueuePanel_Drop: QueuePanel is null, cannot refresh UI");
                                }
                            });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop: Invalid indices - Source: {sourceIndex}, Target: {targetIndex}, QueueCount: {playQueue.Count}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("QueuePanel_Drop: No target queue item found at drop position");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("QueuePanel_Drop: No QueueIndex data found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"QueuePanel_Drop error stack trace: {ex.StackTrace}");
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void UpdateNowPlayingDisplay(AudioLibraryTrack track)
        {
            if (NowPlayingTitle != null && NowPlayingArtist != null && NowPlayingAlbumArt != null)
            {
                NowPlayingTitle.Text = track.Title ?? "Unknown Title";
                NowPlayingArtist.Text = track.Artist ?? "Unknown Artist";
                NowPlayingAlbumArt.Source = GetAlbumArt(track.Path);
            }
        }

        private void TrackItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && element.DataContext is AudioLibraryTrack track)
            {
                // Get only tracks from the same album as the clicked track
                var albumTracks = _all
                    .Where(t => string.Equals(t.Album, track.Album, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.Title) // You might want to order by track number instead
                    .ToList();
                
                var list = albumTracks.Select(t => t.Path).ToList();
                var idx = list.FindIndex(p => string.Equals(p, track.Path, StringComparison.OrdinalIgnoreCase));
                if (idx < 0) idx = 0;
                
                System.Diagnostics.Debug.WriteLine($"TrackItem_MouseDown: Queuing {list.Count} tracks from album '{track.Album}'");
                PlayQueueFrom(list, idx);
            }
        }

        private List<AudioLibraryTrack> _all = new List<AudioLibraryTrack>();
        private List<AudioLibraryTrack> _view = new List<AudioLibraryTrack>();
        private LibrarySettings? _settings;
        private List<string> trackHistory = new List<string>();
        private List<ArtistListItem> artistItems = new List<ArtistListItem>();
        private Button? selectedArtistButton = null;
        private ArtistListItem? currentArtist = null;

        // Internal audio player state
        private IWavePlayer? wavePlayer;
        private AudioFileReader? audioFileReader;
        private DispatcherTimer? timer;
        private bool isPlaying = false;
        private bool isDraggingSlider = false;
        private List<string> playQueue = new();
        private int currentIndex = -1;
        private string? currentAlbum = null; // Track current album for queue display
        private bool isSwitchingTrack = false;
        private bool suppressPlaybackStopped = false;

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

        private void UpdatePlayPauseButton()
        {
            var playPauseButton = GetUIElement<Button>("LibPlayPauseButton");
            if (playPauseButton != null)
            {
                playPauseButton.Content = _playbackManager.IsPlaying ? "[ PAUSE ]" : "[ PLAY ]";
            }
        }

        private void UpdateTimeline()
        {
            var timeline = GetUIElement<Slider>("LibTimeline");
            var currentTime = GetUIElement<TextBlock>("LibCurrentTime");
            
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
            var timeline = GetUIElement<Slider>("LibTimeline");
            var totalTime = GetUIElement<TextBlock>("LibTotalTime");
            
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
                // Update current track display in UI
                var trackInfo = GetUIElement<TextBlock>("TrackInfo");
                if (trackInfo != null)
                {
                    trackInfo.Text = $"{_playbackManager.CurrentTrack.Artist} - {_playbackManager.CurrentTrack.Title}";
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MusicLibraryWindow()
        {
            InitializeComponent();
            InitializeUI();
            SetupPlaybackManagerBindings();
        }

        private void InitializeUI()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MusicLibraryWindow: Constructor started");
                System.Diagnostics.Debug.WriteLine("MusicLibraryWindow: InitializeComponent completed");
                
                // Set up panel-level drop handling for queue items
                var queuePanel = GetUIElement<System.Windows.Controls.StackPanel>("QueuePanel");
                if (queuePanel != null)
                {
                    queuePanel.AllowDrop = true;
                    queuePanel.Drop += QueuePanel_Drop;
                    queuePanel.DragEnter += QueuePanel_DragEnter;
                    queuePanel.DragLeave += QueuePanel_DragLeave;
                    System.Diagnostics.Debug.WriteLine("MusicLibraryWindow: QueuePanel drop handlers set up");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MusicLibraryWindow: QueuePanel is null in constructor");
                }
                
                _settings = LibrarySettingsStore.Load() ?? new LibrarySettings { Folders = LibrarySettingsStore.GetDefaultFolders() };
                System.Diagnostics.Debug.WriteLine("MusicLibraryWindow: Settings loaded");
                
                Loaded += async (s, e) => 
                {
                    System.Diagnostics.Debug.WriteLine("MusicLibraryWindow: Loaded event triggered");
                    await ScanAsync();
                };
                
                timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                timer.Tick += Timer_Tick;
                System.Diagnostics.Debug.WriteLine("MusicLibraryWindow: Constructor completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MusicLibraryWindow Constructor error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
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

        private async Task ScanAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ScanAsync: Starting scan");
                if (StatusText != null)
                {
                    StatusText.Text = "Scanning...";
                }
                System.Diagnostics.Debug.WriteLine("ScanAsync: Status text set");
                
                var folders = _settings?.Folders ?? LibrarySettingsStore.GetDefaultFolders();
                if (_settings == null)
                {
                    System.Diagnostics.Debug.WriteLine("ScanAsync: _settings was null, using default folders");
                }
                System.Diagnostics.Debug.WriteLine($"ScanAsync: Got {folders.Count} folders");
                
                _all = await LibraryScanner.ScanAsync(folders);
                System.Diagnostics.Debug.WriteLine($"ScanAsync: Scanned {_all.Count} tracks");
                
                ApplyFilter(SearchBox.Text);
                System.Diagnostics.Debug.WriteLine("ScanAsync: Filter applied");
                
                PopulateArtists();
                System.Diagnostics.Debug.WriteLine("ScanAsync: Artists populated");
                
                // FoldersTree functionality removed in new MusicBee-style layout
                System.Diagnostics.Debug.WriteLine("ScanAsync: Folders tree populated");
                
                if (StatusText != null)
                {
                    StatusText.Text = $"Found {_all.Count} tracks";
                }
                System.Diagnostics.Debug.WriteLine("ScanAsync: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ScanAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (StatusText != null)
                {
                    StatusText.Text = $"Scan failed: {ex.Message}";
                }
            }
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            await ScanAsync();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(SearchBox.Text);
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "search")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 239));
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "search";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void ApplyFilter(string? text)
        {
            string q = text?.Trim() ?? string.Empty;
            
            // Start with the current artist's tracks if one is selected, otherwise all tracks
            var searchBase = currentArtist != null ? 
                _all.Where(t => string.Equals(string.IsNullOrWhiteSpace(t.Artist) ? "Unknown Artist" : t.Artist,
                                          currentArtist.Name,
                                          StringComparison.OrdinalIgnoreCase)).ToList() :
                _all.ToList();
            
            if (string.IsNullOrEmpty(q))
            {
                _view = searchBase;
            }
            else
            {
                var cmp = StringComparison.OrdinalIgnoreCase;
                _view = searchBase.Where(t =>
                    (t.Title?.IndexOf(q, cmp) ?? -1) >= 0 ||
                    (t.Artist?.IndexOf(q, cmp) ?? -1) >= 0 ||
                    (t.Album?.IndexOf(q, cmp) ?? -1) >= 0 ||
                    (t.Path?.IndexOf(q, cmp) ?? -1) >= 0
                ).ToList();
            }
            
            // Update status text to reflect search scope
            if (StatusText != null)
            {
                if (currentArtist != null)
                {
                    StatusText.Text = string.IsNullOrEmpty(q) ? 
                        $"Showing {searchBase.Count} tracks by {currentArtist.Name}" : 
                        $"Found {_view.Count} of {searchBase.Count} tracks by {currentArtist.Name}";
                }
                else
                {
                    StatusText.Text = string.IsNullOrEmpty(q) ? 
                        $"Showing {_view.Count} of {_all.Count}" : 
                        $"Found {_view.Count} of {_all.Count}";
                }
            }
            
            // Refresh the display with filtered results
            RefreshAlbumsDisplay();
        }


        // Search box changed in XAML -> ApplyFilter

        private async void ManageFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ManageFoldersWindow();
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                _settings = LibrarySettingsStore.Load();
                await ScanAsync();
            }
        }

        private void AddToTrackHistory(string trackPath)
        {
            var track = _all.FirstOrDefault(t => string.Equals(t.Path, trackPath, StringComparison.OrdinalIgnoreCase));
            if (track != null)
            {
                string trackInfo = $"{track.Title} — {track.Artist}";
                trackHistory.Remove(trackInfo); // Remove if exists to move to top
                trackHistory.Insert(0, trackInfo);
                if (trackHistory.Count > 50) trackHistory.RemoveAt(trackHistory.Count - 1); // Keep only 50 items
                // TrackHistoryList functionality moved to QueuePanel in new layout
            }
        }

        private void PopulateArtists()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PopulateArtists: Starting");
                
                artistItems = _all.GroupBy(t => string.IsNullOrWhiteSpace(t.Artist) ? "Unknown Artist" : t.Artist,
                                           StringComparer.OrdinalIgnoreCase)
                                  .OrderBy(g => g.Key)
                                  .Select(g => new ArtistListItem
                                  {
                                      Name = g.Key,
                                      TrackCount = g.Count()
                                  })
                                  .ToList();
                System.Diagnostics.Debug.WriteLine($"PopulateArtists: Created {artistItems.Count} artist items");
                
                // Update ArtistsPanel with artist buttons
                if (ArtistsPanel != null)
                {
                    ArtistsPanel.Children.Clear();
                    foreach (var artist in artistItems.OrderBy(a => a.Name))
                    {
                        var button = new Button
                        {
                            Content = artist.Name,
                            Background = System.Windows.Media.Brushes.Transparent,
                            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 239)),
                            FontSize = 12,
                            Padding = new Thickness(8, 4, 8, 4),
                            Margin = new Thickness(4, 2, 4, 2),
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Tag = artist
                        };
                        button.Click += ArtistButton_Click;
                        ArtistsPanel.Children.Add(button);
                    }
                    System.Diagnostics.Debug.WriteLine("PopulateArtists: ArtistsPanel populated");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("PopulateArtists: ArtistsPanel is null!");
                }
                
                // Select first artist by default
                if (artistItems.Count > 0)
                {
                    DisplayArtist(artistItems[0]);
                    // Highlight the first artist button
                    var firstButton = ArtistsPanel.Children.Cast<Button>().FirstOrDefault();
                    if (firstButton != null)
                    {
                        selectedArtistButton = firstButton;
                        firstButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 64, 73)); // Darker shade
                    }
                }
                System.Diagnostics.Debug.WriteLine("PopulateArtists: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PopulateArtists error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private BitmapImage GetAlbumArt(string? trackPath)
        {
            try
            {
                if (string.IsNullOrEmpty(trackPath) || !File.Exists(trackPath))
                {
                    System.Diagnostics.Debug.WriteLine($"GetAlbumArt: Track file not found: {trackPath}");
                    return GetDefaultAppIcon();
                }

                System.Diagnostics.Debug.WriteLine($"GetAlbumArt: Loading album art for {trackPath}");
                
                // Use TagLib# to extract album art
                var file = TagLib.File.Create(trackPath);
                if (file.Tag.Pictures.Length > 0)
                {
                    var picture = file.Tag.Pictures[0];
                    using (var stream = new MemoryStream(picture.Data.Data))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        System.Diagnostics.Debug.WriteLine("GetAlbumArt: Successfully loaded album art");
                        return bitmap;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("GetAlbumArt: No album art found in file");
                    return GetDefaultAppIcon();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAlbumArt error: {ex.Message}");
                return GetDefaultAppIcon();
            }
        }

        private BitmapImage GetDefaultAppIcon()
        {
            try
            {
                // Try multiple approaches to load the icon
                string[] possiblePaths = {
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"),
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Debug", "net8.0-windows", "icon.ico"),
                    "icon.ico",
                    "/icon.ico"
                };

                foreach (var iconPath in possiblePaths)
                {
                    if (System.IO.File.Exists(iconPath))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri(System.IO.Path.GetFullPath(iconPath));
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();
                        return bmp;
                    }
                }

                // Try pack URI as last resort
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri("pack://application:,,,/icon.ico");
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    return bmp;
                }
                catch { }
            }
            catch { }
            
            // Fallback: create a simple colored rectangle
            var drawing = new System.Windows.Media.DrawingVisual();
            using (var context = drawing.RenderOpen())
            {
                context.DrawRectangle(System.Windows.Media.Brushes.DarkGray, null, new System.Windows.Rect(0, 0, 80, 80));
            }
            var renderTarget = new RenderTargetBitmap(80, 80, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            renderTarget.Render(drawing);
            
            // Convert RenderTargetBitmap to BitmapImage
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            bitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTarget));
            
            using (var stream = new System.IO.MemoryStream())
            {
                bitmapEncoder.Save(stream);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            
            return bitmapImage;
        }

        // BuildFolderChildren functionality removed in new MusicBee-style layout

        private void ArtistButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ArtistListItem artist)
            {
                // Reset previous selection
                if (selectedArtistButton != null)
                {
                    selectedArtistButton.Background = System.Windows.Media.Brushes.Transparent;
                }
                
                // Highlight new selection
                selectedArtistButton = button;
                button.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 64, 73)); // Darker shade
                
                DisplayArtist(artist);
            }
        }

        private void DisplayArtist(ArtistListItem item)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DisplayArtist: Starting for '{item.Name}'");
                
                // Ensure we're on UI thread
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(() => DisplayArtist(item));
                    return;
                }
                
                // Set current artist
                currentArtist = item;
                
                var artistName = item.Name;
                var tracks = _all.Where(t => string.Equals(string.IsNullOrWhiteSpace(t.Artist) ? "Unknown Artist" : t.Artist,
                                                           artistName,
                                                           StringComparison.OrdinalIgnoreCase)).ToList();
                System.Diagnostics.Debug.WriteLine($"DisplayArtist: Found {tracks.Count} tracks");
                
                _view = tracks;
                
                // Clear any existing search when selecting a new artist
                if (SearchBox != null && SearchBox.Text == "search")
                {
                    RefreshAlbumsDisplay();
                }
                else
                {
                    // Apply current search filter to the new artist selection
                    ApplyFilter(SearchBox.Text);
                }
                
                System.Diagnostics.Debug.WriteLine($"DisplayArtist: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DisplayArtist error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        private void RefreshAlbumsDisplay()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("RefreshAlbumsDisplay: Starting");
                
                var albums = _view.Where(t => !string.IsNullOrWhiteSpace(t.Album))
                                   .GroupBy(t => t.Album)
                                   .OrderBy(g => g.Key)
                                   .ToList();
                
                // Update AlbumsPanel with album and track information
                if (AlbumsPanel != null)
                {
                    AlbumsPanel.Children.Clear();
                
                    foreach (var albumGroup in albums)
                    {
                        // Album header
                        var albumHeader = new TextBlock
                        {
                            Text = albumGroup.Key,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 239)),
                            Margin = new Thickness(0, 8, 0, 4)
                        };
                        AlbumsPanel.Children.Add(albumHeader);
                    
                        // Album tracks
                        foreach (var track in albumGroup.OrderBy(t => t.Title))
                        {
                            var trackBorder = new Border
                            {
                                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 70, 80)),
                                Margin = new Thickness(0, 1, 0, 1),
                                Padding = new Thickness(8, 4, 8, 4),
                                Cursor = System.Windows.Input.Cursors.Hand,
                                DataContext = track
                            };
                            trackBorder.MouseEnter += (s, e) => trackBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 82, 92));
                            trackBorder.MouseLeave += (s, e) => trackBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 70, 80));
                            trackBorder.MouseLeftButtonDown += TrackItem_MouseDown;
                        
                            var trackStack = new StackPanel { Orientation = Orientation.Horizontal };
                            // TrackNumber removed since AudioLibraryTrack doesn't have this property
                            trackStack.Children.Add(new TextBlock { Text = track.Title, FontSize = 12, Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 235, 239)), VerticalAlignment = VerticalAlignment.Center });
                        
                            trackBorder.Child = trackStack;
                            AlbumsPanel.Children.Add(trackBorder);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("RefreshAlbumsDisplay: AlbumsPanel is null!");
                }
                
                System.Diagnostics.Debug.WriteLine($"RefreshAlbumsDisplay: Populated {albums.Count} albums with {_view.Count} tracks");
                System.Diagnostics.Debug.WriteLine("RefreshAlbumsDisplay: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshAlbumsDisplay error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void AlbumToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Removed
        }

        // PopupTrack_MouseDown functionality removed in new MusicBee-style layout

        private void ProfileTab_Click(object sender, RoutedEventArgs e)
        {
            // No longer needed since tabs were removed
        }

        private void UpdateTabSelection(string selectedTab)
        {
            // No longer needed since tabs were removed
        }

        // TrackHistoryList functionality moved to QueuePanel in new layout
        // This method will be updated to work with the new queue system

        // FoldersTree_SelectedItemChanged functionality removed in new MusicBee-style layout

        private void NowPlayingList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This method is now handled by TrackHistoryList_MouseDoubleClick
        }

        private void UpdateDataGridSelection(string currentTrackPath)
        {
            var track = _view.FirstOrDefault(t => string.Equals(t.Path, currentTrackPath, StringComparison.OrdinalIgnoreCase));
            if (track != null)
            {
            }
        }

        private void SwitchToMiniPlayer_Click(object sender, RoutedEventArgs e)
        {
            AudioLibraryTrack? track = null;
            if (_view.Count > 0)
            {
                track = _view[0];
            }
            if (track != null)
            {
                var player = new AudioPlayerWindow(track.Path);
                player.Owner = this;
                player.Show();
            }
        }

        // UpdateInfoPanel functionality removed in new MusicBee-style layout

        // UpdateInfoPanelFromPath functionality removed in new MusicBee-style layout

        private void PlayQueueFrom(List<string> paths, int startIndex)
        {
            System.Diagnostics.Debug.WriteLine($"PlayQueueFrom: Starting with {paths?.Count ?? 0} paths, startIndex: {startIndex}");
            
            playQueue = paths ?? new List<string>();
            if (playQueue.Count == 0) return;
            if (startIndex < 0 || startIndex >= playQueue.Count) startIndex = 0;
            currentIndex = startIndex;
            
            // Set current album based on the selected track
            var currentTrack = _all.FirstOrDefault(t => string.Equals(t.Path, playQueue[currentIndex], StringComparison.OrdinalIgnoreCase));
            if (currentTrack != null)
            {
                currentAlbum = currentTrack.Album;
                System.Diagnostics.Debug.WriteLine($"PlayQueueFrom: Set current album to '{currentAlbum}' for track '{currentTrack.Title}'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"PlayQueueFrom: Could not find track at index {currentIndex}");
            }
            
            SwitchAndPlay(playQueue[currentIndex]);
            UpdateQueueDisplay(); // Sync queue display with current selection
        }

        private void SetNowPlayingListFromPaths(IEnumerable<string> paths)
        {
            
        }

        private void SwitchAndPlay(string path)
        {
            if (isSwitchingTrack) return;
            isSwitchingTrack = true;
            suppressPlaybackStopped = true;
            timer?.Stop();
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

            try
            {
                audioFileReader = new AudioFileReader(path);
                wavePlayer = new WaveOutEvent();
                wavePlayer.Init(audioFileReader);
                wavePlayer.Volume = (float)(LibVolumeSlider.Value / 100.0);
                wavePlayer.PlaybackStopped += WavePlayer_PlaybackStopped;
                LibTimeline.Maximum = audioFileReader.TotalTime.TotalSeconds;
                LibTotalTime.Text = FormatTime(audioFileReader.TotalTime);
                // UpdateInfoPanelFromPath functionality removed in new MusicBee-style layout
                UpdateDataGridSelection(path);
                AddToTrackHistory(path);

                audioFileReader.Position = 0;
                suppressPlaybackStopped = false;
                wavePlayer.Play();
                timer?.Start();
                LibPlayPauseButton.Content = "[ PAUSE ]";
                isPlaying = true;
                
                // Update now playing display
                var currentTrack = _all.FirstOrDefault(t => string.Equals(t.Path, path, StringComparison.OrdinalIgnoreCase));
                if (currentTrack != null)
                {
                    currentAlbum = currentTrack.Album; // Update current album
                    UpdateNowPlayingDisplay(currentTrack);
                }
                
                // Update queue display
                UpdateQueueDisplay();
            }
            catch (Exception ex)
            {
                if (StatusText != null)
                {
                    StatusText.Text = $"Error: {ex.Message}";
                }
            }
            finally
            {
                isSwitchingTrack = false;
            }
        }

        private void WavePlayer_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (suppressPlaybackStopped) return;
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("WavePlayer_PlaybackStopped: Event triggered");
                timer?.Stop();
                LibPlayPauseButton.Content = "[ PLAY ]";
                isPlaying = false;
                
                // Check if track has finished playing (allow for small timing differences)
                bool trackFinished = false;
                if (audioFileReader != null)
                {
                    // Check if we're very close to the end (within 0.5 seconds)
                    var remainingTime = audioFileReader.TotalTime - audioFileReader.CurrentTime;
                    trackFinished = remainingTime.TotalSeconds <= 0.5;
                    System.Diagnostics.Debug.WriteLine($"WavePlayer_PlaybackStopped: Remaining time: {remainingTime.TotalSeconds:F2}s, Track finished: {trackFinished}");
                }
                
                if (trackFinished)
                {
                    System.Diagnostics.Debug.WriteLine($"WavePlayer_PlaybackStopped: Attempting auto-next. Queue count: {playQueue.Count}, Current index: {currentIndex}");
                    // Auto next - advance to next track in queue
                    if (playQueue.Count > 0 && currentIndex >= 0 && currentIndex < playQueue.Count)
                    {
                        // Move to next track in queue
                        int nextIndex = currentIndex + 1;
                        if (nextIndex >= playQueue.Count)
                        {
                            nextIndex = 0; // Loop back to start
                        }
                        System.Diagnostics.Debug.WriteLine($"WavePlayer_PlaybackStopped: Moving to index {nextIndex}");
                        currentIndex = nextIndex;
                        SwitchAndPlay(playQueue[currentIndex]);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"WavePlayer_PlaybackStopped: Cannot auto-next - invalid queue state");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WavePlayer_PlaybackStopped: Track not finished, not auto-advancing");
                }
            });
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (audioFileReader != null && !isDraggingSlider && LibTimeline != null && LibCurrentTime != null)
                {
                    LibTimeline.Value = audioFileReader.CurrentTime.TotalSeconds;
                    LibCurrentTime.Text = FormatTime(audioFileReader.CurrentTime);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Timer_Tick error: {ex.Message}");
                timer?.Stop();
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}";
        }

        private void MiniPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide library window and show miniplayer
            this.Hide();
            
            if (_miniPlayerWindow == null || !_miniPlayerWindow.IsLoaded)
            {
                _miniPlayerWindow = new AudioPlayerWindow();
                _miniPlayerWindow.Closed += (s, args) => 
                {
                    _miniPlayerWindow = null;
                    this.Show();
                };
            }
            
            _miniPlayerWindow.Show();
        }

        private void LibPrevButton_Click(object sender, RoutedEventArgs e)
        {
            _playbackManager.Previous();
        }

        private void LibPlayPauseButton_Click(object sender, RoutedEventArgs e)
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

        private void LibNextButton_Click(object sender, RoutedEventArgs e)
        {
            _playbackManager.Next();
        }

        private void LibTimeline_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void LibTimeline_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDraggingSlider = false;
            _playbackManager.SeekTo(LibTimeline.Value);
        }

        private void LibTimeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDraggingSlider)
            {
                var currentTime = GetUIElement<TextBlock>("LibCurrentTime");
                if (currentTime != null)
                {
                    currentTime.Text = FormatTime(TimeSpan.FromSeconds(LibTimeline.Value));
                }
            }
        }

        private void LibTimeline_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (audioFileReader != null)
            {
                var slider = (Slider)sender;
                var pos = e.GetPosition(slider);
                var pct = pos.X / slider.ActualWidth;
                var newVal = pct * slider.Maximum;
                slider.Value = newVal;
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(newVal);
            }
        }

        private void LibVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (wavePlayer != null)
            {
                wavePlayer.Volume = (float)(LibVolumeSlider.Value / 100.0);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer?.Stop();
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
