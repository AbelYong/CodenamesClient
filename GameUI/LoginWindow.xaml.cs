using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CodenamesClient.GameUI
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private MainMenuWindow _mainMenuWindow;
        public LoginWindow()
        {
            InitializeComponent();
            _mainMenuWindow = new MainMenuWindow();
        }

        private void Click_btnLogin(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not avaible yet");
        }

        private void Click_btnSignIn(object sender, RoutedEventArgs e)
        {
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Owner = this;
            signInWindow.ShowDialog();
        }

        private void Click_btnPlayAsGuest(object sender, RoutedEventArgs e)
        {
            stackPanelLogin.Visibility = Visibility.Collapsed;
            CurrentContent.Content = _mainMenuWindow;
        }
    }
}
