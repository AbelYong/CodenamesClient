using CodenamesGame.Domain.POCO;
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
    /// Lógica de interacción para ProfileWindow.xaml
    /// </summary>
    public partial class ProfileWindow : Window
    {
        private PlayerPOCO _player;
        private int _avatarID;
        public ProfileWindow(PlayerPOCO player)
        {
            InitializeComponent();
            this._player = player;
            FillProfileFields(player);
        }

        public void Click_btnSave(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public void Click_btnProfilePicture(object sender, RoutedEventArgs e)
        {

        }

        public void Click_btnBack(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;

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
                //TODO profile image handling
            }
        }
    }
}
