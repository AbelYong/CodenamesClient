using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Operation;
using CodenamesClient.Operation.Network.Oneway;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Util;
using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CodenamesClient.GameUI.Pages.UserControls
{
    public partial class ProfileControl : UserControl
    {
        private readonly ProfileViewModel _viewModel;
        private readonly PlayerDM _player;
        private string _auxAuthReason;
        private int _tempAvatarID;
        public event Action ClickCloseProfile;
        public event Action ClickSaveProfile;

        public ProfileControl(PlayerDM player, bool isReadOnly)
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel();
            DataContext = _viewModel;
            _player = player;
            _tempAvatarID = player.AvatarID;
            FillProfileFields(player);

            if (isReadOnly)
            {
                SetReadOnlyMode();
            }
        }

        private void FillProfileFields(PlayerDM player)
        {
            if (player != null)
            {
                tBxUsername.Text = player.Username;
                tBxEmail.Text = player.User.Email;
                tBxName.Text = player.Name;
                tBxLastName.Text = player.LastName;
                tBxFacebook.Text = player.FacebookUsername;
                tBxInstagram.Text = player.InstagramUsername;
                tBxDiscord.Text = player.DiscordUsername;
                SetProfilePicture(player.AvatarID);
            }
        }

        public void Click_btnBack(object sender, RoutedEventArgs e)
        {
            ClickCloseProfile?.Invoke();
        }

        private void SetReadOnlyMode()
        {
            void DisableInteraction(TextBox tb)
            {
                if (tb == null)
                {
                    return;
                }
                tb.IsReadOnly = true;
                tb.IsHitTestVisible = false;
                tb.Focusable = false;
            }

            DisableInteraction(tBxUsername);
            DisableInteraction(tBxEmail);
            DisableInteraction(tBxName);
            DisableInteraction(tBxLastName);
            DisableInteraction(tBxFacebook);
            DisableInteraction(tBxInstagram);
            DisableInteraction(tBxDiscord);

            btnProfilePicture.IsEnabled = false;

            if (btnSave != null)
            {
                btnSave.Visibility = Visibility.Collapsed;
            }
            if (tBxChangePassword != null)
            {
                tBxChangePassword.Visibility = Visibility.Collapsed;
            }
            if (lblPassword != null)
            {
                lblPassword.Visibility = Visibility.Collapsed;
            }
            if (VerifyEmailLabel != null)
            {
                VerifyEmailLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowGridAuthenticate(AuthtenticationReason reason)
        {
            _auxAuthReason = reason.ToString();
            stackPanelProfileForm.Visibility = Visibility.Hidden;
            var slideInAnimation = (Storyboard)FindResource("SlideInAuthenticateAnimation");
            gridAuthenticate.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideGridAuthenticate(bool isCancelAuth)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutAuthenticateAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                gridAuthenticate.Visibility = Visibility.Collapsed;
                stackPanelProfileForm.Visibility = isCancelAuth ? Visibility.Visible : Visibility.Hidden;
            };
            slideOutAnimation.Begin();
        }

        private void Click_btnAuthenticate(object sender, RoutedEventArgs e)
        {
            bool parseSuccess = Enum.TryParse(_auxAuthReason, out AuthtenticationReason reason);
            if (parseSuccess)
            {
                if (Authenticate())
                {
                    switch (reason)
                    {
                        case AuthtenticationReason.EMAIL_UPDATE:
                            HideGridAuthenticate(isCancelAuth: false);
                            VerifyEmail();
                            ClearPasswordFields();
                            break;
                        case AuthtenticationReason.PASSWORD_RESET:
                            HideGridAuthenticate(isCancelAuth: false);
                            ShowGridResetPassword();
                            break;
                        default:
                            return;
                    }
                }
            }
            else
            {
                MessageBox.Show(Lang.globalClientError);
            }
        }

        private bool Authenticate()
        {
            CodenamesGame.AuthenticationService.AuthenticationRequest request =
                OnewayNetworkManager.Instance.Authenticate(_player.Username, _viewModel.CurrentPassword);
            if (request.IsSuccess)
            {
                return true;
            }
            else
            {
                MessageBox.Show(StatusToMessageMapper.GetAuthServiceMessage(AuthOperationType.AUTHENTICATION, request.StatusCode));
                return false;
            }
        }

        private void Click_btnCancelAuthentication(object sender, RoutedEventArgs e)
        {
            HideGridAuthenticate(isCancelAuth: true);
            ClearPasswordFields();
        }

        private void ClearPasswordFields()
        {
            _viewModel.CurrentPassword = string.Empty;
            _viewModel.NewPassword = string.Empty;
            _viewModel.ConfirmPassword = string.Empty;
        }

        public void Click_btnSave(object sender, RoutedEventArgs e)
        {
            var errors = Validation.ProfileValidation.ValidateAll(
                username: tBxUsername.Text?.Trim(),
                email: tBxEmail.Text?.Trim(),
                firstName: tBxName.Text?.Trim(),
                lastName: tBxLastName.Text?.Trim(),
                facebook: tBxFacebook.Text?.Trim(),
                instagram: tBxInstagram.Text?.Trim(),
                discord: tBxDiscord.Text?.Trim()
            );

            var list = new List<string>(errors);
            if (list.Count > 0)
            {
                MessageBox.Show(string.Join("\n", list), Lang.globalInvalidData,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (_player.User.Email.Equals(tBxEmail.Text))
                {
                    SaveProfile();
                    ClickSaveProfile?.Invoke();
                }
                else
                {
                    ShowGridAuthenticate(AuthtenticationReason.EMAIL_UPDATE);
                }
            }
        }

        private void SaveProfile()
        {
            PlayerDM updatedPlayer = PrepareUpdatedPlayer();
            CodenamesGame.UserService.CommunicationRequest request = OnewayNetworkManager.Instance.UpdateProfile(updatedPlayer);
            string message = request.StatusCode == CodenamesGame.UserService.StatusCode.NOT_FOUND ?
                Lang.profileUpdateErrorProfileNotFound : StatusToMessageMapper.GetUserServiceMessage(request.StatusCode);
            MessageBox.Show(message);
        }

        private void VerifyEmail()
        {
            bool wasCodeSent = SendVerificationCode(tBxEmail.Text);
            if (wasCodeSent)
            {
                ShowgGridVerify();
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

        private void Click_btnConfirmVerify(object sender, EventArgs e)
        {
            string newEmail = tBxEmail.Text;
            string code = tbxVerifyCode.Text;
            CodenamesGame.EmailService.ConfirmEmailRequest request =
                OnewayNetworkManager.Instance.SendVerificationCode(newEmail, code, CodenamesGame.EmailService.EmailType.EMAIL_VERIFICATION);
            if (request.IsSuccess)
            {
                SaveProfile();
                ClickSaveProfile?.Invoke();
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
            }
        }

        private void ShowgGridVerify()
        {
            stackPanelProfileForm.Visibility = Visibility.Hidden;
            _viewModel.EmailVerification = tBxEmail.Text;
            var slideInAnimation = (Storyboard)FindResource("SlideInVerifyAnimation");
            gridVerify.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void Click_btnHideVerify(object sender, RoutedEventArgs e)
        {
            HideGridVerify();
        }

        private void HideGridVerify()
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutVerifyAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                gridVerify.Visibility = Visibility.Collapsed;
                stackPanelProfileForm.Visibility = Visibility.Visible;
            };
            slideOutAnimation.Begin();
        }

        private void Click_btnResetPassword(object sender, RoutedEventArgs e)
        {
            ShowGridAuthenticate(AuthtenticationReason.PASSWORD_RESET);
        }

        private void ShowGridResetPassword()
        {
            stackPanelProfileForm.Visibility = Visibility.Hidden;
            var slideInAnimation = (Storyboard)FindResource("SlideInResetPasswordAnimation");
            gridResetPassword.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void Click_btnHideResetPassword(object sender, RoutedEventArgs e)
        {
            HideGridResetPassword();
        }

        private void Click_btnConfirmResetPassword(object sender, RoutedEventArgs e)
        {
            CodenamesGame.AuthenticationService.CommunicationRequest request =
                OnewayNetworkManager.Instance.UpdatePassword(_player.Username, _viewModel.CurrentPassword, _viewModel.NewPassword);
            if (request.IsSuccess)
            {
                MessageBox.Show(Lang.profilePasswordHasBeenUpdated);
                HideGridResetPassword();
            }
            else
            {
                MessageBox.Show(StatusToMessageMapper.GetAuthServiceMessage(AuthOperationType.PASS_UPDATE, request.StatusCode));
            }
            ClearPasswordFields();
        }

        private void HideGridResetPassword()
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutResetPasswordAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                gridResetPassword.Visibility = Visibility.Collapsed;
                stackPanelProfileForm.Visibility = Visibility.Visible;
            };
            slideOutAnimation.Begin();
        }

        private void PasswordInput_LostFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.TriggerPasswordValidation();
        }

        private void Click_btnProfilePicture(object sender, RoutedEventArgs e)
        {
            ShowGridProfilePictures();
        }

        private void Click_btnSelectProfilePicture(object sender, RoutedEventArgs e)
        {
            int numRows = 5;
            if (sender is Button clickedPicture)
            {
                int row = Grid.GetRow(clickedPicture);
                int column = Grid.GetColumn(clickedPicture);

                int imageIndex = (row * numRows) + (column);

                SetProfilePicture(imageIndex);
            }
            HideGridProfilePictures();
        }

        private void SetProfilePicture(int avatarID)
        {
            _tempAvatarID = avatarID;
            btnProfilePicture.Background = PictureHandler.GetImage(avatarID);
        }

        private void ShowGridProfilePictures()
        {
            _viewModel.IsTitleVisible = Visibility.Hidden;
            stackPanelProfileForm.Visibility = Visibility.Hidden;
            var slideInAnimation = (Storyboard)FindResource("SlideInAnimation");
            gridProfilePictures.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideGridProfilePictures()
        {
            _viewModel.IsTitleVisible = Visibility.Visible;
            var slideOutAnimation = (Storyboard)FindResource("SlideOutAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                gridProfilePictures.Visibility = Visibility.Collapsed;
                stackPanelProfileForm.Visibility = Visibility.Visible;
            };
            slideOutAnimation.Begin();
        }

        

        private PlayerDM PrepareUpdatedPlayer()
        {
            PlayerDM updatedPlayer = new PlayerDM();
            updatedPlayer.User.UserID = _player.User.UserID;
            updatedPlayer.User.Email = tBxEmail.Text;

            updatedPlayer.PlayerID = _player.PlayerID;
            updatedPlayer.Username = tBxUsername.Text;
            updatedPlayer.AvatarID = _tempAvatarID;
            updatedPlayer.Name = (!string.IsNullOrEmpty(tBxName.Text) ? tBxName.Text : null);
            updatedPlayer.LastName = (!string.IsNullOrEmpty(tBxLastName.Text) ? tBxLastName.Text : null);
            updatedPlayer.FacebookUsername = (!string.IsNullOrEmpty(tBxFacebook.Text) ? tBxFacebook.Text : null);
            updatedPlayer.InstagramUsername = (!string.IsNullOrEmpty(tBxInstagram.Text) ? tBxInstagram.Text : null);
            updatedPlayer.DiscordUsername = (!string.IsNullOrEmpty(tBxDiscord.Text) ? tBxDiscord.Text : null);
            return updatedPlayer;
        }

        private enum AuthtenticationReason
        {
            EMAIL_UPDATE,
            PASSWORD_RESET
        }
    }
}