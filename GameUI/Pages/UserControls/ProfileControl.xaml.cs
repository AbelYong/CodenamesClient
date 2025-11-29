using CodenamesClient.Operation;
using CodenamesClient.Properties.Langs;
using CodenamesClient.Util;
using CodenamesGame.Domain.POCO;
using CodenamesGame.EmailService;
using CodenamesGame.Network;
using CodenamesGame.UserService;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CodenamesClient.GameUI.Pages.UserControls
{
    public partial class ProfileControl : UserControl
    {
        private PlayerDM _player;
        private int _tempAvatarID;
        public event Action ClickCloseProfile;
        public event Action ClickSaveProfile;

        public ProfileControl(PlayerDM player)
        {
            InitializeComponent();
            _player = player;
            _tempAvatarID = player.AvatarID;
            FillProfileFields(player);
        }

        public void Click_btnSave(object sender, RoutedEventArgs e)
        {
            var errors = CodenamesClient.Validation.ProfileValidation.ValidateAll(
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
                System.Windows.MessageBox.Show(string.Join("\n", list), Lang.globalInvalidData,
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
                    VerifyEmail();
                }
            }
        }

        private void SaveProfile()
        {
            PlayerDM updatedPlayer = PrepareUpdatedPlayer();
            CodenamesGame.UserService.CommunicationRequest request = UserOperation.UpdateProfile(updatedPlayer);
            MessageBox.Show(Util.StatusToMessageMapper.GetUserServiceMessage(request.StatusCode));
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
            CodenamesGame.EmailService.CommunicationRequest request = EmailOperation.SendVerificationEmail(email);
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
            CodenamesGame.EmailService.ConfirmEmailRequest request = EmailOperation.SendVerificationCode(newEmail, code);
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

        public void Click_btnBack(object sender, RoutedEventArgs e)
        {
            ClickCloseProfile?.Invoke();
        }

        private void Click_btnProfilePicture(object sender, RoutedEventArgs e)
        {
            stackPanelProfileForm.Visibility = Visibility.Hidden;
            var slideInAnimation = (Storyboard)FindResource("SlideInAnimation");
            gridProfilePictures.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void Click_btnSelectProfilePicture(object sender, RoutedEventArgs e)
        {
            const int NUM_ROWS = 5;
            if (sender is Button clickedButton)
            {
                int row = Grid.GetRow(clickedButton);
                int column = Grid.GetColumn(clickedButton);

                int imageIndex = (row * NUM_ROWS) + (column);

                _tempAvatarID = imageIndex;
                SetProfilePicture(_tempAvatarID);
            }
            var slideOutAnimation = (Storyboard)FindResource("SlideOutAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                gridProfilePictures.Visibility = Visibility.Collapsed;
                stackPanelProfileForm.Visibility = Visibility.Visible;
            };
            slideOutAnimation.Begin();
        }

        private void SetProfilePicture(int avatarID)
        {
            btnProfilePicture.Background = PictureHandler.GetImage(avatarID);
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

        private PlayerDM PrepareUpdatedPlayer()
        {
            PlayerDM updatedPlayer = new PlayerDM();
            updatedPlayer.User.UserID = _player.User.UserID;
            updatedPlayer.User.Email = tBxEmail.Text;

            updatedPlayer.PlayerID = _player.PlayerID;
            updatedPlayer.Username = tBxUsername.Text;
            updatedPlayer.AvatarID = _tempAvatarID;
            updatedPlayer.Name = (!String.IsNullOrEmpty(tBxName.Text) ? tBxName.Text : null);
            updatedPlayer.LastName = (!String.IsNullOrEmpty(tBxLastName.Text) ? tBxLastName.Text : null);
            updatedPlayer.FacebookUsername = (!String.IsNullOrEmpty(tBxFacebook.Text) ? tBxFacebook.Text : null);
            updatedPlayer.InstagramUsername = (!String.IsNullOrEmpty(tBxInstagram.Text) ? tBxInstagram.Text : null);
            updatedPlayer.DiscordUsername = (!String.IsNullOrEmpty(tBxDiscord.Text) ? tBxDiscord.Text : null);
            return updatedPlayer;
        }
    }
}
