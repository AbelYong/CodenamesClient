using CodenamesClient.Operation;
using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.UserService;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CodenamesClient.GameUI
{
    /// <summary>
    /// Lógica de interacción para ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private PlayerPOCO _player;
        private int _tempAvatarID;

        public ProfileWindow(PlayerPOCO player)
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
                return;
            }
            else
            {
                PlayerPOCO updatedPlayer = PrepareUpdatedPlayer();
                UpdateResult result = UserOperations.UpdateProfile(updatedPlayer);
                MessageBox.Show(result.Message);
                this.DialogResult = result.Success;
            }
        }

        public void Click_btnBack(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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

        private void FillProfileFields(PlayerPOCO player)
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
                //TODO address handling
                SetProfilePicture(player.AvatarID);
            }
        }

        private PlayerPOCO PrepareUpdatedPlayer()
        {
            PlayerPOCO updatedPlayer = new PlayerPOCO();
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
