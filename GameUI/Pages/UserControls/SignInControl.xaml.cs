using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Properties.Langs;
using System;
using Sv = CodenamesGame.AuthenticationService;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CodenamesClient.GameUI.Pages.UserControls
{
    public partial class SignInControl : UserControl
    {
        private readonly SignInViewModel _vm;
        private Guid? _pendingRequestId = null;
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

        private async void Click_SignIn(object sender, RoutedEventArgs e)
        {
            _vm.ValidateAll();
            if (!_vm.CanSubmit) return;

            try
            {
                using (var client = new Sv.AuthenticationManagerClient())
                {
                    var svUser = new Sv.User
                    { 
                        Email = _vm.Email,
                        Password = _vm.Password 
                    };
                    var svPlayer = new Sv.Player
                    { 
                        Username = _vm.Username, 
                        Name = _vm.FirstName, 
                        LastName = _vm.LastName 
                    };

                    var begin = await Task.Run(() => client.BeginRegistration(svUser, svPlayer, _vm.Password));

                    if (begin == null || !begin.Success || !begin.RequestId.HasValue)
                    {
                        MessageBox.Show(begin?.Message ?? Lang.signInRegistrationCouldNotBeStarted);
                        return;
                    }

                    _pendingRequestId = begin.RequestId.Value;

                    VerifyEmailLabel.Text = _vm.Email;
                    VerifyCode.Text = string.Empty;
                    ShowVerifyOverlay();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.signInErrorStartingRegistration + ex.Message);
            }
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

                if (VerifyGrid.RenderTransform is TranslateTransform tt) tt.Y = 800;
            };

            sb.Completed += onDone;
            sb.Begin(VerifyGrid, true);
        }

        private async void ConfirmVerify_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            try
            {
                var code = (VerifyCode.Text ?? "").Trim();
                if (code.Length != 6 || !code.All(char.IsDigit))
                {
                    MessageBox.Show(Lang.signInCodeMustHave6Digits);
                    return;
                }
                if (_pendingRequestId == null)
                {
                    MessageBox.Show(Lang.signInNoPendingRequests);
                    return;
                }

                using (var client = new Sv.AuthenticationManagerClient())
                {
                    var result = await Task.Run(() =>
                        client.ConfirmRegistration(_pendingRequestId.Value, code)
                    );

                    MessageBox.Show(result.Message,
                                    Lang.signInRegister,
                                    MessageBoxButton.OK,
                                    result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Lang.signInErrorConfirmingRegistration + ex.Message);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private async void HideVerify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pendingRequestId != null)
                {
                    using (var client = new Sv.AuthenticationManagerClient())
                    {
                        await Task.Run(() => client.CancelRegistration(_pendingRequestId.Value));
                    }
                }
            }
            catch
            {
                /* best effort */
            }
            finally
            {
                _pendingRequestId = null;
                HideVerifyOverlay();
            }
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
