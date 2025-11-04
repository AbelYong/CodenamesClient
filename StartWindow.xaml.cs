using CodenamesClient.GameUI;
using System.Diagnostics;
using System.Windows;

namespace CodenamesClient
{
    /// <summary>
    /// Lógica de interacción para StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void Click_btnEnglish(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "en-US";
            Properties.Settings.Default.Save();
            Restart();
        }

        private void Click_btnSpanish(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.languageCode = "es-MX";
            Properties.Settings.Default.Save();
            Restart();
        }


        private void Click_btnStart(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private static void Restart()
        {
            string executionPath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(executionPath);
            Application.Current.Shutdown();
        }
    }
}
