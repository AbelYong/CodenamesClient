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
        private readonly LoginViewModel _viewModel;

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
            AudioManager.Instance.StopAllAudio();
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
            ResetEmail.Text = string.Empty;
            ResetCode.Text = string.Empty;
            NewPassword.Password = string.Empty;
            ConfirmPassword.Password = string.Empty;
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

                if (ResetGrid.RenderTransform is TranslateTransform tt)
                {
                    tt.Y = 640;
                }
            };

            sb.Completed += onDone;

            sb.Begin(ResetGrid, true);
        }

        private void SendCode_Click(object sender, RoutedEventArgs e)
        {
            ToggleSendButtonEnabled(sender);

            var email = ResetEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(Lang.signInEmailRequired);
                return;
            }

            CodenamesGame.EmailService.CommunicationRequest request = LoginViewModel.SendPasswordResetEmail(email);

            if (request.IsSuccess)
            {
                MessageBox.Show(Lang.resetCodeSend);
            }
            else
            {
                string message = request.StatusCode == CodenamesGame.EmailService.StatusCode.NOT_FOUND ? 
                    Lang.emailPasswordResetFailedAdressNotFound : StatusToMessageMapper.GetEmailServiceMessage(request.StatusCode);
                MessageBox.Show(message);
            }

            ToggleSendButtonEnabled(sender);
        }

        private void ToggleSendButtonEnabled(object sender)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = !btn.IsEnabled;
            }
        }

        private async void ConfirmReset_Click(object sender, RoutedEventArgs e)
        {
            string email = ResetEmail.Text.Trim();
            string code = ResetCode.Text.Trim();
            string newPassword = NewPassword.Password.Trim();
            string confirmPassword = ConfirmPassword.Password.Trim();

            bool passwordsMatch = ValidatePasswordsMatch(newPassword, confirmPassword);
            if (!passwordsMatch)
            {
                MessageBox.Show(Lang.resetPasswordsDoNotMatch);
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < ProfileValidation.PASSWORD_MIN_LENGTH || newPassword.Length > ProfileValidation.PASSWORD_MAX_LENGTH)
            {
                MessageBox.Show(Lang.resetPasswordLengthInvalid);
                return;
            }

            var request = await Task.Run(() =>
                LoginViewModel.CompletePasswordReset(email, code, newPassword)
            );

            if (request.IsSuccess)
            {
                MessageBox.Show(Lang.profilePasswordHasBeenUpdated);
                HideResetOverlay();
            }
            else
            {
                string message = StatusToMessageMapper.GetAuthServiceMessage(AuthOperationType.PASS_RESET, request.StatusCode);
                if (request.StatusCode == CodenamesGame.AuthenticationService.StatusCode.UNAUTHORIZED)
                {
                    message = request.RemainingAttempts > 0 ? string.Format(message, request.RemainingAttempts) : Lang.emailConfirmationCodeExpiredOrRemoved;
                }
                MessageBox.Show(message);
            }
        }

        private static bool ValidatePasswordsMatch(string newPassword, string confirmPassword)
        {
            return newPassword == confirmPassword;
        }
    }
}
