using System;
using System.Windows;

namespace BobMediaPlayer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Check if a file was passed as command-line argument
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                string ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
                bool isAudio = ext == ".mp3" || ext == ".wav" || ext == ".m4a" || ext == ".wma";

                if (isAudio)
                {
                    // Open audio player directly to avoid flashing the main window
                    var audioWindow = new AudioPlayerWindow(filePath);
                    // Treat the audio window as the main window so closing it exits the app
                    MainWindow = audioWindow;
                    audioWindow.Show();
                }
                else
                {
                    // For video/images or other types, use MainWindow flow
                    var mainWindow = new MainWindow();
                    MainWindow = mainWindow;
                    mainWindow.Show();
                    mainWindow.LoadMediaFromCommandLine(filePath);
                }
            }
            else
            {
                // Normal startup - show MainWindow
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
            }
        }
    }
}
