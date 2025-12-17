using System;
using System.Windows;
namespace BobMediaPlayer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length > 0)
            {
                string pathtofile = e.Args[0];
                string ext = System.IO.Path.GetExtension(pathtofile).ToLowerInvariant();
                bool isAudio = ext == ".mp3" || ext == ".wav" || ext == ".m4a" || ext == ".wma";

                if (isAudio)
                {
                    // Audio files are now handled by BobMusicPlayer - launch that instead
                    System.Diagnostics.Process.Start("BobMusicPlayer.exe", $"\"{pathtofile}\"");
                    Shutdown();
                    return;
                }
                else
                {
                    var mainWindow = new MainWindow();
                    MainWindow = mainWindow;
                    mainWindow.Show();
                    mainWindow.LoadMediaFromCommandLine(pathtofile);
                }
            }
            else
            {
                // Launch the main window - music library is now in separate app
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
            }
        }
    }
}
