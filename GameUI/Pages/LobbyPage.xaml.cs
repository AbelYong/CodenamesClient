using CodenamesClient.GameUI.BoardUI;
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
using System.Xml.Linq;

namespace CodenamesClient.GameUI.Pages
{
    /// <summary>
    /// Lógica de interacción para LobbyPage.xaml
    /// </summary>
    public partial class LobbyPage : Page
    {
        private LobbyViewModel _viewModel;
        private GamemodeDM.GamemodeName _gamemodeName;
        private PlayerDM _player;

        public LobbyPage(PlayerDM player, GamemodeDM gamemode)
        {
            InitializeComponent();
            this._viewModel = new LobbyViewModel(gamemode);
            this.DataContext = _viewModel;
            _gamemodeName = gamemode.name;
            _player = player;
        }

        private void Click_StartGame(object sender, RoutedEventArgs e)
        {
            MatchDM match = PrepareMatch();
            BoardPage board = new BoardPage(match);
            NavigationService.Navigate(board);
        }

        private void Click_ReturnToLobby(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private MatchDM PrepareMatch()
        {
            MatchDM match = new MatchDM();
            match.Rules = new GamemodeDM(_gamemodeName)
            {
                turnTimer = _viewModel.TurnTimer,
                timerTokens = _viewModel.TimerTokens,
                bystanderTokens = _viewModel.BystanderTokens,
            };
            match.Player = _player;
            match.Companion = new PlayerDM(); //TODO get the real friend/stranger
            return match;
        }
    }
}
