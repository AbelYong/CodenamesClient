using CodenamesClient.GameUI.BoardUI;
using CodenamesClient.GameUI.ViewModels;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.MatchmakingService;
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
        private GamemodeDM _gamemode;
        private PlayerDM _player;

        public LobbyPage(PlayerDM player, GamemodeDM gamemode)
        {
            InitializeComponent();
            this._viewModel = new LobbyViewModel(gamemode);
            this.DataContext = _viewModel;
            _gamemode = gamemode;
            _player = player;
        }

        private void Click_StartGame(object sender, RoutedEventArgs e)
        {
            MatchConfigurationDM matchConfig = PrepareMatchRequest();
            
        }

        private void Click_ReturnToLobby(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private MatchConfigurationDM PrepareMatchRequest()
        {
            MatchConfigurationDM matchConfig = new MatchConfigurationDM();
            switch (_gamemode)
            {
                case GamemodeDM.NORMAL:
                    matchConfig.Rules.gamemode = GamemodeDM.NORMAL;
                    break;
                case GamemodeDM.CUSTOM:
                    matchConfig.Rules.gamemode = GamemodeDM.CUSTOM;
                    matchConfig.Rules.TurnTimer = _viewModel.TurnTimer;
                    matchConfig.Rules.TimerTokens = _viewModel.TimerTokens;
                    matchConfig.Rules.BystanderTokens = _viewModel.BystanderTokens;
                    break;
                case GamemodeDM.COUNTERINTELLIGENCE:
                    matchConfig.Rules.gamemode = GamemodeDM.COUNTERINTELLIGENCE;
                    break;
                default:
                    matchConfig.Rules.gamemode = GamemodeDM.NORMAL;
                    break;
            }
            matchConfig.Requester = _player;
            matchConfig.Companion = LoginViewModel.AssembleGuest(); //TODO get the real friend/stranger
            return matchConfig;
        }
    }
}
