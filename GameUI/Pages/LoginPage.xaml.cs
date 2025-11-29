using CodenamesClient.GameUI.Pages.UserControls;
using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Validation;
using CodenamesGame.AuthenticationService;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CodenamesClient.GameUI.Pages
{
    public partial class LoginPage : Page
    {
        private LoginViewModel _viewModel;
        
        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            _viewModel.ConnectionLost += ShowConnectionLostMessage;
        }

        private void ShowConnectionLostMessage()
        {
            string message = _viewModel.RequestErrorMessage;
            MessageBox.Show(message);
        }

        private async void Click_btnLogin(object sender, RoutedEventArgs e)
        {
            string username = tBxUsername.Text;
            string password = pBxPassword.Password;

            if (ValidateLoginData(username, password))
            {
                LoginRequest request = AuthenticationOperation.Authenticate(username, password);
                if (request.IsSuccess)
                {
                    await BeginSession(request.UserID);
                }
                else
                {
                    if (request.StatusCode == StatusCode.UNAUTHORIZED)
                    {
                        ShowFailedLoginMessage();
                    }
                    else
                    {
                        MessageBox.Show(Util.StatusToMessageMapper.GetAuthServiceMessage(request.StatusCode));
                    }
                }
            }
        }

        private void Click_btnSignIn(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            SignInControl.Show();
        }

        private void Click_SignInClose(object sender, RoutedEventArgs e)
        {
            SignInControl.Hide(() =>
            {
                Overlay.Visibility = Visibility.Collapsed;
            });
        }

        private async void Click_btnPlayAsGuest(object sender, RoutedEventArgs e)
        {
            await BeginSession(null);
        }

        private async Task BeginSession(Guid? userID)
        {
            if (userID != null)
            {
                Guid auxUserID = userID.Value;
                PlayerDM player = UserOperation.GetPlayer(auxUserID);
                await _viewModel.Connect(player);
                GoToMainMenuWindow(player, false);
            }
            else
            {
                PlayerDM guest = LoginViewModel.AssembleGuest();
                await _viewModel.Connect(guest);
                GoToMainMenuWindow(guest, true);
            }
        }

        private void GoToMainMenuWindow(PlayerDM player, bool isGuest)
        {
            if (_viewModel.HasPlayerConnection)
            {
                MainMenuPage mainMenu = new MainMenuPage(player, isGuest);
                NavigationService.Navigate(mainMenu);
            }
        }

        private bool ValidateLoginData(string username, string password)
        {
            ClearFields();
            string usernameMessage = LoginValidation.ValidateUsername(username);
            lblUsernameErrorMessage.Content = (usernameMessage.Equals("OK") ? "" : usernameMessage);

            string passwordMessage = LoginValidation.ValidatePassword(password);
            lblPasswordErrorMessage.Content = (passwordMessage.Equals("OK") ? "" : passwordMessage);

            return usernameMessage.Equals("OK") && passwordMessage.Equals("OK");
        }

        private void ClearFields()
        {
            lblUsernameErrorMessage.Content = "";
            lblPasswordErrorMessage.Content = "";
        }
        private void ShowFailedLoginMessage()
        {
            lblUsernameErrorMessage.Content = Lang.loginWrongCredentials;
            lblPasswordErrorMessage.Content = Lang.loginWrongCredentials;
        }

        private void ForgotLink_Click(object sender, MouseButtonEventArgs e)
        {
            PrefillResetFields();
            ShowResetOverlay();
        }

        private void HideReset_Click(object sender, RoutedEventArgs e)
        {
            HideResetOverlay();
        }
        private void HideReset_Backdrop(object sender, MouseButtonEventArgs e)
        {
            HideResetOverlay();
        } 

        private void PrefillResetFields()
        {
            if (string.IsNullOrWhiteSpace(ResetUsername.Text))
            {
                ResetUsername.Text = tBxUsername.Text;
            }

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
            if (btn != null)
            {
                btn.IsEnabled = false;
            }

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
                    AuthenticationOperation.BeginPasswordReset(user, email)
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
                    AuthenticationOperation.CompletePasswordReset(user, code, p1)
                );

                MessageBox.Show(result.Message);
                if (result.Success)
                {
                    HideResetOverlay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.resetPasswordChangeFailed + ": " + ex.Message);
            }
            finally
            {
                if (btn != null)
                {
                    btn.IsEnabled = true;
                }
            }
        }
    }
}
