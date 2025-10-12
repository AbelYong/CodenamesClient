using CodenamesClient.GameUI.ViewModels;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
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
    /// Lógica de interacción para SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        private readonly SignInViewModel _vm = new SignInViewModel();

        public SignInWindow()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        private void Click_SignIn(object sender, RoutedEventArgs e)
        {
            _vm.ValidateAll();
            if (_vm.CanSubmit)
            {
                UserPOCO newUser = AssembleDmUser();
                PlayerPOCO newPlayer = AssembleDmPlayer();
                Guid? newUserID = UserOperations.SignIn(newUser, newPlayer);
                if (newUserID != null)
                {
                    MessageBox.Show("Welcome to codenames, agent "+newPlayer.Username+"!");
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Sorry, we couldn't sign you in, please try again later. Contact the developers if the issue persists");
                    DialogResult= false;
                }
            }
        }

        private UserPOCO AssembleDmUser()
        {
            UserPOCO user = new UserPOCO();
            user.Email = _vm.Email;
            user.Password = _vm.Password;
            return user;
        }
        private PlayerPOCO AssembleDmPlayer()
        {
            PlayerPOCO player = new PlayerPOCO();
            player.Username = _vm.Username;
            player.Name = _vm.FirstName;
            player.LastName = _vm.LastName;
            return player;
        }
    }
}
