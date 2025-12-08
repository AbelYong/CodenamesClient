using CodenamesClient.GameUI.Pages.UserControls;
using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Util;
using CodenamesClient.Validation;
using CodenamesGame.Domain.POCO;
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
        private int _remainingResetAttempts;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            Loaded += OnLoginPageLoaded;
            Unloaded += OnLoginPageUnloaded;
        }

        private void OnLoginPageLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.RaiseError += ShowErrorMessage;
            _viewModel.NavigateToMainMenu += GoToMainMenuWindow;
        }

        private void OnLoginPageUnloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.RaiseError -= ShowErrorMessage;
            _viewModel.NavigateToMainMenu -= GoToMainMenuWindow;
        }

        private void ShowErrorMessage()
        {
            string message = _viewModel.RequestErrorMessage;
            MessageBox.Show(message);
        }

        private async void Click_btnLogin(object sender, RoutedEventArgs e)
        {
            await _viewModel.Login(tBxUsername.Text, pBxPassword.Password);
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
            await _viewModel.BeginSession(null);
        }

        private void GoToMainMenuWindow(object sender, PlayerDM player)
        {
            if (_viewModel.HasPlayerConnection)
            {
                MainMenuPage mainMenu = new MainMenuPage(player, player.IsGuest);
                _viewModel.RaiseError -= ShowErrorMessage;
                NavigationService.Navigate(mainMenu);
            }
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
            ResetEmail.Text = "";
            ResetCode.Text = "";
            NewPassword.Password = "";
            ConfirmPassword.Password = "";
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

        private void SendCode_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                btn.IsEnabled = false;
            }

            try
            {
                var email = ResetEmail.Text.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(Lang.signInEmailRequired);
                    return;
                }

                CodenamesGame.EmailService.CommunicationRequest request = LoginViewModel.SendPasswordResetEmail(email);

                if (request.IsSuccess)
                {
                    int initialResetAttempts = 3;
                    _remainingResetAttempts = initialResetAttempts;
                    MessageBox.Show(Lang.resetCodeSend);
                }
                else
                {
                    MessageBox.Show(StatusToMessageMapper.GetEmailServiceMessage(request.StatusCode));
                }
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
            var email = ResetEmail.Text.Trim();
            var code = ResetCode.Text.Trim();
            var p1 = NewPassword.Password;
            var p2 = ConfirmPassword.Password;

            if (p1 != p2)
            {
                MessageBox.Show(Lang.resetPasswordsDoNotMatch);
                return;
            }
            if (string.IsNullOrWhiteSpace(p1) || p1.Length < ProfileValidation.PASSWORD_MIN_LENGTH || p1.Length > ProfileValidation.PASSWORD_MAX_LENGTH)
            {
                MessageBox.Show(Lang.resetPasswordLengthInvalid);
                return;
            }

            var result = await Task.Run(() =>
                LoginViewModel.CompletePasswordReset(email, code, p1)
            );

            if (result.IsSuccess)
            {
                MessageBox.Show(Lang.profilePasswordHasBeenUpdated);
                HideResetOverlay();
            }
            else
            {
                _remainingResetAttempts = _remainingResetAttempts == 0 ? 0 : _remainingResetAttempts - 1; //fixme
                string message = StatusToMessageMapper.GetAuthServiceMessage(AuthOperationType.PASS_RESET, result.StatusCode);
                if (result.StatusCode == CodenamesGame.AuthenticationService.StatusCode.UNAUTHORIZED)
                {
                    message = string.Format(message, _remainingResetAttempts);
                }
                MessageBox.Show(message);
            }
        }
    }
}
