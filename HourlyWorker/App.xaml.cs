using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HourlyWorker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static Mutex mutex;

        private const string AppUniqueName = "HourlyWorker";

        //Check if an instance of this application is running and if so show an error and close.
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool isNewInstance = true;
            mutex = new Mutex(true, AppUniqueName, out isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show("Already an instance is running...", "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            mutex.ReleaseMutex();
            mutex.Close();
            mutex = null;
        }
    }
}
