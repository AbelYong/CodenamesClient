using CodenamesClient.Properties.Langs;
using CodenamesClient.Validation;
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
            string username = tBxUsername.Text;
            string password = pBxPassword.Password;

            if (ValidateLoginData(username, password))
            {
                Guid? userID = CodenamesGame.Network.UserOperations.Authenticate(username, password);
                if (userID != null)
                {
                    GoToMainMenuWindow(userID);
                }
                else
                {
                    ShowFailedLoginMessage();
                }
            }
        }

        private void Click_btnSignIn(object sender, RoutedEventArgs e)
        {
            SignInWindow signInWindow = new SignInWindow();
            signInWindow.Owner = this;
            signInWindow.ShowDialog();
        }

        private void Click_btnPlayAsGuest(object sender, RoutedEventArgs e)
        {
            GoToMainMenuWindow(null);
        }

        private void GoToMainMenuWindow(Guid? userID)
        {
            stackPanelLogin.Visibility = Visibility.Collapsed;
            CurrentContent.Content = _mainMenuWindow;
            _mainMenuWindow.setPlayer(userID);
        }

        private bool ValidateLoginData(string username, string password)
        {
            ClearFields();
            string usernameMessage = LoginValidation.ValidateUsername(username);
            if (!usernameMessage.Equals("OK"))
            {
                lblUsernameErrorMessage.Content = usernameMessage;
            }

            string passwordMessage = LoginValidation.ValidatePassword(password);
            if (!passwordMessage.Equals("OK"))
            {
                lblPasswordErrorMessage.Content = passwordMessage;
            }

            return (usernameMessage.Equals("OK") && passwordMessage.Equals("OK"));
        }

        private void ShowFailedLoginMessage()
        {
            lblUsernameErrorMessage.Content = Lang.loginWrongCredentials;
            lblPasswordErrorMessage.Content = Lang.loginWrongCredentials;
        }

        private void ClearFields()
        {
            lblUsernameErrorMessage.Content = "";
            lblPasswordErrorMessage.Content = "";
        }
    }
}
