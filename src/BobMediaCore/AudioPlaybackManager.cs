using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using NAudio.Wave;

namespace BobMediaPlayer.Library
{
    public class AudioPlaybackManager : INotifyPropertyChanged
    {
        private static AudioPlaybackManager? _instance;
        private static readonly object _lock = new object();

        public static AudioPlaybackManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AudioPlaybackManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private AudioPlaybackManager()
        {
            InitializeAudio();
        }

        private AudioFileReader? _audioFile;
        private IWavePlayer? _wavePlayer;
        private AudioLibraryTrack? _currentTrack;
        private List<AudioLibraryTrack> _playQueue = new List<AudioLibraryTrack>();
        private int _currentQueueIndex = -1;
        private bool _isPlaying = false;
        private bool _isDragging = false;
        private double _volume = 0.5;
        private double _position = 0;
        private double _duration = 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? TrackChanged;
        public event EventHandler? PlaybackStateChanged;
        public event EventHandler? QueueChanged;
        public event EventHandler? PositionChanged;

        public AudioLibraryTrack? CurrentTrack
        {
            get => _currentTrack;
            private set
            {
                if (_currentTrack != value)
                {
                    _currentTrack = value;
                    OnPropertyChanged();
                    TrackChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged();
                    PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double Volume
        {
            get => _volume;
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    OnPropertyChanged();
                    if (_wavePlayer != null)
                    {
                        _wavePlayer.Volume = (float)_volume;
                    }
                }
            }
        }

        public double Position
        {
            get => _position;
            private set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                    PositionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double Duration
        {
            get => _duration;
            private set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<AudioLibraryTrack> PlayQueue
        {
            get => _playQueue;
            private set
            {
                if (_playQueue != value)
                {
                    _playQueue = value;
                    OnPropertyChanged();
                    QueueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int CurrentQueueIndex
        {
            get => _currentQueueIndex;
            private set
            {
                if (_currentQueueIndex != value)
                {
                    _currentQueueIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private void InitializeAudio()
        {
            try
            {
                _wavePlayer = new WaveOutEvent();
                _wavePlayer.Volume = (float)_volume;
                _wavePlayer.PlaybackStopped += OnPlaybackStopped;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize audio: {ex.Message}");
            }
        }

        public async Task<bool> LoadTrackAsync(AudioLibraryTrack track)
        {
            try
            {
                Stop();
                
                if (_wavePlayer != null)
                {
                    _wavePlayer.Dispose();
                }
                
                _audioFile = new AudioFileReader(track.Path);
                _wavePlayer = new WaveOutEvent();
                _wavePlayer.Volume = (float)_volume;
                _wavePlayer.Init(_audioFile);
                
                CurrentTrack = track;
                Duration = _audioFile.TotalTime.TotalSeconds;
                Position = 0;
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load track: {ex.Message}");
                return false;
            }
        }

        public void Play()
        {
            try
            {
                if (_wavePlayer != null && _audioFile != null)
                {
                    _wavePlayer.Play();
                    IsPlaying = true;
                    StartPositionTimer();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play: {ex.Message}");
            }
        }

        public void Pause()
        {
            try
            {
                _wavePlayer?.Pause();
                IsPlaying = false;
                StopPositionTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to pause: {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                _wavePlayer?.Stop();
                IsPlaying = false;
                Position = 0;
                StopPositionTimer();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to stop: {ex.Message}");
            }
        }

        public void Next()
        {
            if (_currentQueueIndex < _playQueue.Count - 1)
            {
                _currentQueueIndex++;
                if (_currentQueueIndex < _playQueue.Count)
                {
                    LoadTrackAsync(_playQueue[_currentQueueIndex]).ContinueWith(_ => Play());
                }
            }
        }

        public void Previous()
        {
            if (_currentQueueIndex > 0)
            {
                _currentQueueIndex--;
                if (_currentQueueIndex >= 0)
                {
                    LoadTrackAsync(_playQueue[_currentQueueIndex]).ContinueWith(_ => Play());
                }
            }
        }

        public void SetQueue(List<AudioLibraryTrack> queue, int startIndex = 0)
        {
            PlayQueue = new List<AudioLibraryTrack>(queue);
            CurrentQueueIndex = startIndex;
        }

        public void SeekTo(double position)
        {
            try
            {
                if (_audioFile != null)
                {
                    _audioFile.CurrentTime = TimeSpan.FromSeconds(position);
                    Position = position;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to seek: {ex.Message}");
            }
        }

        private System.Timers.Timer? _positionTimer;

        private void StartPositionTimer()
        {
            _positionTimer = new System.Timers.Timer(100);
            _positionTimer.Elapsed += UpdatePosition;
            _positionTimer.Start();
        }

        private void StopPositionTimer()
        {
            _positionTimer?.Stop();
            _positionTimer?.Dispose();
            _positionTimer = null;
        }

        private void UpdatePosition(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_audioFile != null && !_isDragging)
            {
                Position = _audioFile.CurrentTime.TotalSeconds;
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            IsPlaying = false;
            StopPositionTimer();
            
            // Auto-play next track
            if (_currentQueueIndex < _playQueue.Count - 1)
            {
                Next();
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Stop();
            _wavePlayer?.Dispose();
            _audioFile?.Dispose();
            StopPositionTimer();
        }
    }
}
