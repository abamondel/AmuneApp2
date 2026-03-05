using System.Threading;
using System.Windows;

namespace AmuneApp
{
    public partial class App : Application
    {
        private static Mutex _mutex;

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

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
