using CodenamesClient.Validation;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CodenamesClient.Properties.Langs;

namespace CodenamesClient.GameUI
{
    public partial class LoginWindow : Window
    {
        private MainMenuWindow _mainMenuWindow;

        public LoginWindow()
        {
            InitializeComponent();
            _mainMenuWindow = new MainMenuWindow();
        }

        // ========= LOGIN =========
        private void Click_btnLogin(object sender, RoutedEventArgs e)
        {
            string username = tBxUsername.Text;
            string password = pBxPassword.Password;

            if (ValidateLoginData(username, password))
            {
                MessageBox.Show("loginData was validated");
                Guid? userID = CodenamesGame.Network.UserOperations.Authenticate(username, password);
                MessageBox.Show("User id " + userID?.ToString());
                if (userID != null) GoToMainMenuWindow(userID);
            }
        }

        private void Click_btnSignIn(object sender, RoutedEventArgs e)
        {
            var w = new SignInWindow { Owner = this };
            w.ShowDialog();
        }

        private void Click_btnPlayAsGuest(object sender, RoutedEventArgs e) => GoToMainMenuWindow(null);

        private void GoToMainMenuWindow(Guid? userID)
        {
            stackPanelLogin.Visibility = Visibility.Collapsed;
            CurrentContent.Content = _mainMenuWindow;
            _mainMenuWindow.setPlayer(userID);
        }

        private bool ValidateLoginData(string username, string password)
        {
            ClearFields();
            var uMsg = LoginValidation.ValidateUsername(username);
            if (uMsg != "OK") lblUsernameErrorMessage.Content = uMsg;

            var pMsg = LoginValidation.ValidatePassword(password);
            if (pMsg != "OK") lblPasswordErrorMessage.Content = pMsg;

            return uMsg == "OK" && pMsg == "OK";
        }

        private void ClearFields()
        {
            lblUsernameErrorMessage.Content = "";
            lblPasswordErrorMessage.Content = "";
        }

        // ========= RESET PASSWORD (overlay) =========
        private void ForgotLink_Click(object sender, MouseButtonEventArgs e)
        {
            PrefillResetFields();
            ShowResetOverlay();
        }

        private void HideReset_Click(object sender, RoutedEventArgs e) => HideResetOverlay();
        private void HideReset_Backdrop(object sender, MouseButtonEventArgs e) => HideResetOverlay();

        private void PrefillResetFields()
        {
            if (string.IsNullOrWhiteSpace(ResetUsername.Text))
                ResetUsername.Text = tBxUsername.Text;

            ResetEmail.Text = "";
            ResetCode.Text = "";
            NewPass1.Password = "";
            NewPass2.Password = "";
        }

        private void ShowResetOverlay()
        {
            ResetBackdrop.Visibility = Visibility.Visible;
            ResetGrid.Visibility = Visibility.Visible;

            var sb = (Storyboard)FindResource("SlideInResetAnimation");
            sb.Begin(ResetGrid, true);
        }

        private void HideResetOverlay()
        {
            var sb = (Storyboard)FindResource("SlideOutResetAnimation");

            EventHandler onDone = null;
            onDone = (s, e) =>
            {
                sb.Completed -= onDone;

                ResetBackdrop.Visibility = Visibility.Collapsed;
                ResetGrid.Visibility = Visibility.Collapsed;

                var tt = ResetGrid.RenderTransform as TranslateTransform;
                if (tt != null) tt.Y = 640;
            };

            sb.Completed += onDone;

            sb.Begin(ResetGrid, true);
        }

        private async void SendCode_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            try
            {
                var user = ResetUsername.Text.Trim();
                var email = ResetEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(Lang.resetMissingUserOrEmail);
                    return;
                }

                await Task.Run(() =>
                    CodenamesGame.Network.UserOperations.BeginPasswordReset(user, email)
                );

                MessageBox.Show(Lang.resetCodeSend);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.resetCodeSendFailed + ": " + ex.Message);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private async void ConfirmReset_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            try
            {
                var user = ResetUsername.Text.Trim();
                var code = ResetCode.Text.Trim();
                var p1 = NewPass1.Password;
                var p2 = NewPass2.Password;

                if (p1 != p2)
                {
                    MessageBox.Show(Lang.resetPasswordsDoNotMatch);
                    return;
                }
                if (string.IsNullOrWhiteSpace(p1) || p1.Length < 10 || p1.Length > 16)
                {
                    MessageBox.Show(Lang.resetPasswordLengthInvalid);
                    return;
                }

                var result = await Task.Run(() =>
                    CodenamesGame.Network.UserOperations.CompletePasswordReset(user, code, p1)
                );

                MessageBox.Show(result.Message);
                if (result.Success) HideResetOverlay();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.resetPasswordChangeFailed + ": " + ex.Message);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }
    }
}