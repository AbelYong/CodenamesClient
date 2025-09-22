using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            //TODO
            throw new NotImplementedException();
        }

        private static void Restart()
        {
            string executionPath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(executionPath);
            Application.Current.Shutdown();
        }
    }
}
