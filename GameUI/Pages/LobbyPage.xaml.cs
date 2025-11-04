using CodenamesClient.GameUI.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodenamesClient.GameUI.Pages
{
    /// <summary>
    /// Lógica de interacción para LobbyPage.xaml
    /// </summary>
    public partial class LobbyPage : Page
    {
        private LobbyViewModel _viewModel;
        public LobbyPage(GamemodeDM gamemode)
        {
            InitializeComponent();
            this._viewModel = new LobbyViewModel(gamemode);
            this.DataContext = _viewModel;
        }

        private void Click_ReturnToLobby(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
