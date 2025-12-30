using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Util;
using CodenamesGame.UserService;
using CodenamesGame.Domain.POCO;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CodenamesClient.Operation.Network.Oneway;

namespace CodenamesClient.GameUI.Pages.UserControls
{
    public partial class SignInControl : UserControl
    {
        private readonly SignInViewModel _vm;
        public event RoutedEventHandler ClickClose;

        public SignInControl()
        {
            InitializeComponent();
            _vm = new SignInViewModel();
            DataContext = _vm;
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
            var sb = (Storyboard)FindResource("SlideInMainAnimation");
            sb.Begin(MainRegisterGrid, true);
        }

        public void Hide(Action onCompleted = null)
        {
            var sb = (Storyboard)FindResource("SlideOutMainAnimation");

            EventHandler onDone = null;
            onDone = (s, e) =>
            {
                sb.Completed -= onDone;
                this.Visibility = Visibility.Collapsed;

                if (MainRegisterGrid.RenderTransform is TranslateTransform tt) tt.Y = 720;

                onCompleted?.Invoke();
            };

            sb.Completed += onDone;
            sb.Begin(MainRegisterGrid, true);
        }

        private void Click_SignIn(object sender, RoutedEventArgs e)
        {
            _vm.ValidateAll();
            if (!_vm.CanSubmit)
            {
                return;
            }
            RequestEmailVerification();
        }

        private void RequestEmailVerification()
        {
            bool wasCodeSent = SendVerificationCode(_vm.Email);
            if (wasCodeSent)
            {
                ShowVerifyOverlay();
            }
        }

        private static bool SendVerificationCode(string email)
        {
            CodenamesGame.EmailService.CommunicationRequest request =
                OnewayNetworkManager.Instance.SendVerificationEmail(email, CodenamesGame.EmailService.EmailType.EMAIL_VERIFICATION);
            if (!request.IsSuccess)
            {
                MessageBox.Show(StatusToMessageMapper.GetEmailServiceMessage(request.StatusCode));
            }
            return request.IsSuccess;
        }

        private void ShowVerifyOverlay()
        {
            VerifyBackdrop.Visibility = Visibility.Visible;
            VerifyGrid.Visibility = Visibility.Visible;

            var sb = (Storyboard)FindResource("SlideInVerifyAnimation");
            sb.Begin(VerifyGrid, true);
        }

        private void HideVerifyOverlay()
        {
            var sb = (Storyboard)FindResource("SlideOutVerifyAnimation");

            EventHandler onDone = null;
            onDone = (s, e) =>
            {
                sb.Completed -= onDone;
                VerifyBackdrop.Visibility = Visibility.Collapsed;
                VerifyGrid.Visibility = Visibility.Collapsed;

                if (VerifyGrid.RenderTransform is TranslateTransform tt)
                {
                    tt.Y = 800;
                }
            };

            sb.Completed += onDone;
            sb.Begin(VerifyGrid, true);
        }

        private void ConfirmVerify_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.IsEnabled = false;
                if (SendEmailVerificationCode())
                {
                    RequestSignIn();
                }
                btn.IsEnabled = true;
            }
        }

        private bool SendEmailVerificationCode()
        {
            const int CODE_LENGTH = 6;
            string code = (VerifyCode.Text ?? "").Trim();
            if (code.Length != CODE_LENGTH || !code.All(char.IsDigit))
            {
                MessageBox.Show(Lang.signInCodeMustHave6Digits);
                return false;
            }

            CodenamesGame.EmailService.ConfirmEmailRequest request =
                OnewayNetworkManager.Instance.SendVerificationCode(_vm.Email, code, CodenamesGame.EmailService.EmailType.EMAIL_VERIFICATION);
            if (request.IsSuccess)
            {
                return true;
            }
            else
            {
                string message;
                if (request.StatusCode == CodenamesGame.EmailService.StatusCode.UNAUTHORIZED)
                {
                    message = string.Format(StatusToMessageMapper.GetEmailServiceMessage(request.StatusCode), request.RemainingAttempts);
                }
                else
                {
                    message = StatusToMessageMapper.GetEmailServiceMessage(request.StatusCode);
                }
                MessageBox.Show(message);
                return false;
            }
        }

        private void RequestSignIn()
        {
            UserDM user = new UserDM
            {
                UserID = Guid.Empty,
                Email = _vm.Email,
                Password = _vm.Password,
            };

            PlayerDM player = new PlayerDM
            {
                PlayerID = Guid.Empty,
                Username = _vm.Username,
                Name = _vm.FirstName,
                LastName = _vm.LastName,
            };
            SignInRequest request = OnewayNetworkManager.Instance.SignIn(user, player);
            if (request.IsSuccess)
            {
                HideVerifyOverlay();
                MessageBox.Show(string.Format(Lang.signInSuccessfulWelcome, _vm.Username));
            }
            else
            {
                MessageBox.Show(GetSignInErrorMessage(request));
            }
        }

        private static string GetSignInErrorMessage(SignInRequest request)
        {
            string message;
            switch (request.StatusCode)
            {
                case StatusCode.MISSING_DATA:
                    return Lang.signInErrorMissingData;
                case StatusCode.UNALLOWED:
                    string emailDuplicateMessage = request.IsEmailDuplicate ? Lang.emailCannotUseAddressAlreadyInUse : string.Empty;
                    string usernameInUseMessage = request.IsUsernameDuplicate ? Lang.profileErrorUsernameAlreadyInUse : string.Empty;
                    message = string.Format("{0} \n{1}", emailDuplicateMessage, usernameInUseMessage);
                    return message;
                case StatusCode.WRONG_DATA:
                    string emailInvalidMessage = request.IsEmailValid ? string.Empty : Lang.signInEmailInvalidFormat;
                    string passwordInvalidMessage = request.IsPasswordValid ? string.Empty : Lang.signInInvalidPassword;
                    message = string.Format("{0} \n{1}", emailInvalidMessage, passwordInvalidMessage);
                    return message;
                default:
                    return StatusToMessageMapper.GetUserServiceMessage(request.StatusCode);
            }
        }

        private void HideVerify_Click(object sender, RoutedEventArgs e)
        {
            HideVerifyOverlay();
        }

        private void Click_btnClose(object sender, RoutedEventArgs e)
        {
            ClickClose?.Invoke(this, e);
        }

        private void PasswordInput_LostFocus(object sender, RoutedEventArgs e)
        {
            _vm.TriggerPasswordValidation();
        }
    }
}
