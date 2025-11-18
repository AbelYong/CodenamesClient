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

        private SessionOperation _session;
        private Storyboard _slideInOnlineFriends;
        private Storyboard _slideOutOnlineFriends;
        private Storyboard _slideInTypeCode;
        private Storyboard _slideOutTypeCode;

        public LobbyPage(PlayerDM player, GamemodeDM gamemode, SessionOperation session)
        {
            InitializeComponent();

            _session = session;
            _viewModel = new LobbyViewModel(player, gamemode, _session);
            DataContext = _viewModel;
            Loaded += OnLobbyPageLoaded;
            Unloaded += OnLobbyPageUnloaded;
        }

        private void OnLobbyPageLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LobbyViewModel vm)
            {
                _slideInOnlineFriends = (Storyboard)FindResource("SlideInOnlineFriendsAnimation");
                _slideOutOnlineFriends = (Storyboard)FindResource("SlideOutOnlineFriendsAnimation");
                _slideInTypeCode = (Storyboard)FindResource("SlideInLobbyCodeAnimation");
                _slideOutTypeCode = (Storyboard)FindResource("SlideOutLobbyCodeAnimation");

                _viewModel.SubscribeToSessionEvents();

                _viewModel.BeginMatch += OnBeginMatch;
            }
        }
        private void OnLobbyPageUnloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.UnsubscribeFromSessionEvents();

            _viewModel.BeginMatch -= OnBeginMatch;
        }

        private async void Click_StartGame(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsPartyFull)
            {
                await _viewModel.RequestArrangedMatch();
            }
        }

        private void Click_btnCreateLobby(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateLobby();
        }

        private void OnBeginMatch(MatchDM match)
        {
            _viewModel.BeginMatch -= OnBeginMatch;
            BoardPage board = new BoardPage(match);
            NavigationService.Navigate(board);
        }

        private void Click_ReturnToLobby(object sender, RoutedEventArgs e)
        {
            _viewModel.DisconnectFromLobbyService();
            _viewModel.DisconnectFromMatchmakingService();
            _viewModel.UnsuscribeFromLobbyEvents();
            _viewModel.UnsubscribeFromSessionEvents();
            _viewModel.UnsuscribeFromMatchmakingEvents();
            NavigationService.GoBack();
        }

        private void Click_btnJoinParty(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            gridTypeCode.Visibility = Visibility.Visible;
            _slideInTypeCode.Begin();
        }

        private void Click_btnSendCode(object sender, RoutedEventArgs e)
        {
            CloseTypeCodeGrid();
            _viewModel.JoinParty(tbkInputCode.Text);
        }

        private void Click_btnCloseTypeCode(object sender, RoutedEventArgs e)
        {
            CloseTypeCodeGrid();
        }

        private void CloseTypeCodeGrid()
        {
            EventHandler slideOutHandler = null;

            slideOutHandler = (s, a) =>
            {
                gridTypeCode.Visibility = Visibility.Collapsed;
                Overlay.Visibility = Visibility.Collapsed;

                _slideOutTypeCode.Completed -= slideOutHandler;
            };

            _slideOutTypeCode.Completed += slideOutHandler;

            _slideOutTypeCode.Begin();
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
                Guid friendID = (Guid)friendToInvite.PlayerID;
                _viewModel.InviteToParty(friendID);
            }
        }
    }
}