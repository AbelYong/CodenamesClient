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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CodenamesClient.GameUI.Pages
{
    /// <summary>
    /// Interaction logic for LobbyPage.xaml
    /// </summary>
    public partial class LobbyPage : Page
    {
        private LobbyViewModel _viewModel;
        private GamemodeDM _gamemode;
        private PlayerDM _player;

        private SessionOperation _session;
        private Storyboard _slideInOnlineFriends;
        private Storyboard _slideOutOnlineFriends;

        public LobbyPage(PlayerDM player, GamemodeDM gamemode, SessionOperation session)
        {
            InitializeComponent();

            this._session = session;
            this._viewModel = new LobbyViewModel(gamemode, _session);
            this.DataContext = _viewModel;
            _gamemode = gamemode;
            _player = player;

            _viewModel.SubscribeToSessionEvents();
            _slideInOnlineFriends = (Storyboard)FindResource("SlideInOnlineFriendsAnimation");
            _slideOutOnlineFriends = (Storyboard)FindResource("SlideOutOnlineFriendsAnimation");
        }

        private void Click_StartGame(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void Click_ReturnToLobby(object sender, RoutedEventArgs e)
        {
            _viewModel.UnsubscribeFromSessionEvents();
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
            matchConfig.Companion = LoginViewModel.AssembleGuest();
            return matchConfig;
        }

        /// <summary>
        /// Displays the online friends panel with an animation.
        /// </summary>
        private void Click_ShowOnlineFriends(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            OnlineFriendsGrid.Visibility = Visibility.Visible;
            _slideInOnlineFriends.Begin();
        }

        /// <summary>
        /// Hides the online friends panel.
        /// </summary>
        private void Click_HideOnlineFriends(object sender, RoutedEventArgs e)
        {
            EventHandler slideOutHandler = null;

            slideOutHandler = (s, a) =>
            {
                OnlineFriendsGrid.Visibility = Visibility.Collapsed;
                Overlay.Visibility = Visibility.Collapsed;

                _slideOutOnlineFriends.Completed -= slideOutHandler;
            };

            _slideOutOnlineFriends.Completed += slideOutHandler;

            _slideOutOnlineFriends.Begin();
        }

        /// <summary>
        /// Click event for the “Invite” button in the online friends list.
        /// </summary>
        private void Click_InviteFriend(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is PlayerDM friendToInvite)
            {
                // TODO: Implement the actual invitation logic here.
                MessageBox.Show($"TODO: Enviar invitación a {friendToInvite.Username}");
            }
        }
    }
}