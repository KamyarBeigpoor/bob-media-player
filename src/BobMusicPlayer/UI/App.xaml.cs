using System.Windows;
using System.Linq;

namespace BobMediaPlayer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Check if audio files were passed as command line arguments
        if (e.Args.Length > 0)
        {
            var audioExtensions = new[] { ".mp3", ".wav", ".m4a", ".wma", ".flac", ".aac" };
            var audioFile = e.Args.FirstOrDefault(arg => audioExtensions.Contains(System.IO.Path.GetExtension(arg).ToLowerInvariant()));
            
            if (!string.IsNullOrEmpty(audioFile))
            {
                // Open AudioPlayerWindow for audio files
                var audioPlayer = new AudioPlayerWindow(audioFile);
                audioPlayer.Show();
                return;
            }
        }
        
        // Default: Show MusicLibraryWindow
        var musicLibrary = new MusicLibraryWindow();
        musicLibrary.Show();
    }
}

