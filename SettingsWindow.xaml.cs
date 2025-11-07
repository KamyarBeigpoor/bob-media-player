using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BobMediaPlayer
{
    public partial class SettingsWindow : Window
    {
        public AppSettings Settings { get; private set; }

        public SettingsWindow(AppSettings currentSettings)
        {
            InitializeComponent();
            Settings = currentSettings.Clone();
            LoadSettings();
            
            // Wire up volume slider value displays
            DefaultVolumeSlider.ValueChanged += (s, e) => 
            {
                VolumeValueText.Text = $"{(int)DefaultVolumeSlider.Value}%";
            };
            MusicPlayerDefaultVolumeSlider.ValueChanged += (s, e) => 
            {
                MusicVolumeValueText.Text = $"{(int)MusicPlayerDefaultVolumeSlider.Value}%";
            };
        }

        private void LoadSettings()
        {
            // Playback
            DefaultSpeedComboBox.SelectedIndex = GetSpeedIndex(Settings.DefaultSpeed);
            DefaultVolumeSlider.Value = Settings.DefaultVolume;

            // Window
            AlwaysOnTopCheckBox.IsChecked = Settings.AlwaysOnTop;

            // Music Player
            MusicPlayerAlwaysOnTopCheckBox.IsChecked = Settings.MusicPlayerAlwaysOnTop;
            MusicPlayerShowMetadataCheckBox.IsChecked = Settings.MusicPlayerShowMetadata;
            MusicPlayerShowAlbumArtCheckBox.IsChecked = Settings.MusicPlayerShowAlbumArt;
            MusicPlayerDefaultVolumeSlider.Value = Settings.MusicPlayerDefaultVolume;

            // Advanced
            ShowOSDCheckBox.IsChecked = Settings.ShowOSD;
        }

        private void SaveSettings()
        {
            // Playback
            Settings.DefaultSpeed = GetSpeedFromIndex(DefaultSpeedComboBox.SelectedIndex);
            Settings.DefaultVolume = (int)DefaultVolumeSlider.Value;

            // Window
            Settings.AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? true;

            // Music Player
            Settings.MusicPlayerAlwaysOnTop = MusicPlayerAlwaysOnTopCheckBox.IsChecked ?? true;
            Settings.MusicPlayerShowMetadata = MusicPlayerShowMetadataCheckBox.IsChecked ?? true;
            Settings.MusicPlayerShowAlbumArt = MusicPlayerShowAlbumArtCheckBox.IsChecked ?? true;
            Settings.MusicPlayerDefaultVolume = (int)MusicPlayerDefaultVolumeSlider.Value;

            // Advanced
            Settings.ShowOSD = ShowOSDCheckBox.IsChecked ?? true;
        }

        private int GetSpeedIndex(double speed)
        {
            if (speed <= 0.25) return 0;
            if (speed <= 0.5) return 1;
            if (speed <= 0.75) return 2;
            if (speed <= 1.0) return 3;
            if (speed <= 1.25) return 4;
            if (speed <= 1.5) return 5;
            return 6;
        }

        private double GetSpeedFromIndex(int index)
        {
            return index switch
            {
                0 => 0.25,
                1 => 0.5,
                2 => 0.75,
                3 => 1.0,
                4 => 1.25,
                5 => 1.5,
                6 => 2.0,
                _ => 1.0
            };
        }

        private int GetSubtitleSizeIndex(int size)
        {
            if (size <= 24) return 0;
            if (size <= 32) return 1;
            if (size <= 42) return 2;
            return 3;
        }

        private int GetSubtitleSizeFromIndex(int index)
        {
            return index switch
            {
                0 => 24,
                1 => 32,
                2 => 42,
                3 => 54,
                _ => 32
            };
        }

        private int GetEncodingIndex(string encoding)
        {
            return encoding switch
            {
                "UTF-8" => 0,
                "UTF-16" => 1,
                "ASCII" => 2,
                _ => 0
            };
        }

        private string GetEncodingFromIndex(int index)
        {
            return index switch
            {
                0 => "UTF-8",
                1 => "UTF-16",
                2 => "ASCII",
                _ => "UTF-8"
            };
        }

        private int GetAspectRatioIndex(string ratio)
        {
            return ratio switch
            {
                "AUTO" => 0,
                "16:9" => 1,
                "4:3" => 2,
                "21:9" => 3,
                _ => 0
            };
        }

        private string GetAspectRatioFromIndex(int index)
        {
            return index switch
            {
                0 => "AUTO",
                1 => "16:9",
                2 => "4:3",
                3 => "21:9",
                _ => "AUTO"
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            DialogResult = true;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            Settings = new AppSettings();
            LoadSettings();
        }

    }

    public class AppSettings
    {
        // Playback
        public double DefaultSpeed { get; set; } = 1.0;
        public int DefaultVolume { get; set; } = 50;

        // Window
        public bool AlwaysOnTop { get; set; } = true;

        // Advanced
        public bool ShowOSD { get; set; } = true;

        // Music Player
        public bool MusicPlayerAlwaysOnTop { get; set; } = true;
        public bool MusicPlayerShowMetadata { get; set; } = true;
        public bool MusicPlayerShowAlbumArt { get; set; } = true;
        public int MusicPlayerDefaultVolume { get; set; } = 50;

        public AppSettings Clone()
        {
            return new AppSettings
            {
                DefaultSpeed = this.DefaultSpeed,
                DefaultVolume = this.DefaultVolume,
                AlwaysOnTop = this.AlwaysOnTop,
                ShowOSD = this.ShowOSD,
                MusicPlayerAlwaysOnTop = this.MusicPlayerAlwaysOnTop,
                MusicPlayerShowMetadata = this.MusicPlayerShowMetadata,
                MusicPlayerShowAlbumArt = this.MusicPlayerShowAlbumArt,
                MusicPlayerDefaultVolume = this.MusicPlayerDefaultVolume
            };
        }
    }
}
