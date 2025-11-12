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
                    var audioWindow = new AudioPlayerWindow(pathtofile);
                    MainWindow = audioWindow;
                    audioWindow.Show();
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
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
            }
        }
    }
}
