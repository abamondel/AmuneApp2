using System.IO;
using System.Threading;
using System.Windows;

namespace AmuneApp
{
    public partial class App : Application
    {
        private static Mutex _mutex;
        private static readonly string LogPath = @"C:\ProgramData\AmuneApp\log.txt";

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "AmuneApp_SingleInstance";
            _mutex = new Mutex(true, mutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("AmuneApp is already running.", "AmuneApp",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            // Crash logging
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                LogException(args.ExceptionObject as Exception, "FATAL");
            };

            DispatcherUnhandledException += (s, args) =>
            {
                LogException(args.Exception, "UI ERROR");
                args.Handled = true;
            };

            base.OnStartup(e);
        }

        private static void LogException(Exception ex, string level)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {ex}\n\n");
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
