using CodenamesClient.GameUI.Pages.UserControls;
using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Operation.Network.Oneway;
using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network.EventArguments;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace CodenamesClient.GameUI.Pages
{
    public partial class MainMenuPage : Page
    {
        private MainMenuViewModel _viewModel;
        private ProfileControl _profileControl;

        public MainMenuPage(PlayerDM player, bool isGuest)
        {
            InitializeComponent();

            _viewModel = new MainMenuViewModel(player, isGuest);
            DataContext = _viewModel;
            Loaded += OnMainMenuPageLoaded;
            Unloaded += OnMainMenuPageUnloaded;

            AudioManager.Instance.StartPlayback("Main");
        }

        private void OnMainMenuPageLoaded(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsPlayerGuest)
            {
                _viewModel.ConnectLobbyService();
                _viewModel.OnInvitationReceived += HandleLobbyInvitationReceived;
                _viewModel.SuscribeToLobbyInvitations();
            }
        }

        private void OnMainMenuPageUnloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.OnInvitationReceived -= HandleLobbyInvitationReceived;
            _viewModel.UnsuscribeFromLobbyInvitations();
        }

        private void Click_btnPlayer(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsPlayerGuest)
            {
                Overlay.Visibility = Visibility.Visible;
                _profileControl = new ProfileControl(_viewModel.Player, false);
                _profileControl.VerticalAlignment = VerticalAlignment.Center;
                _profileControl.HorizontalAlignment = HorizontalAlignment.Center;
                _profileControl.Visibility = Visibility.Visible;

                _profileControl.ClickCloseProfile += CloseProfile;
                _profileControl.CloseProfile += SaveProfile;

                gridMainMenu.Children.Add(_profileControl);
            }
            else
            {
                MessageBox.Show(Lang.mainMenuGuestCannotAccess);
            }
        }

        private void SaveProfile()
        {
            SetPlayerAfterProfileUpdate(_viewModel.Player.User.UserID);
            CloseProfile();
        }

        private void CloseProfile()
        {
            if (_profileControl != null)
            {
                _profileControl.ClickCloseProfile -= CloseProfile;
                _profileControl.CloseProfile -= SaveProfile;
                _profileControl.Visibility = Visibility.Collapsed;
                gridMainMenu.Children.Remove(_profileControl);
                _profileControl = null;
                Overlay.Visibility = Visibility.Collapsed;
            }
        }

        private void SetPlayerAfterProfileUpdate(Guid? userID)
        {
            if (userID != null)
            {
                Guid auxUserID = (Guid)userID;
                PlayerDM player = OnewayNetworkManager.Instance.GetPlayer(auxUserID);
                if (player != null && player.PlayerID != Guid.Empty)
                {
                    _viewModel.Player = player;
                    btnPlayer.Content = player.Username;
                }
                else
                {
                    MessageBox.Show(Util.StatusToMessageMapper.GetUserServiceMessage(CodenamesGame.UserService.StatusCode.NOT_FOUND));
                }
            }
        }

        private void Click_btnLogout(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player != null)
            {
                _viewModel.Disconnect();
            }
            LoginPage login = new LoginPage();
            NavigationService.Navigate(login);
        }

        private void Click_btnQuit(object sender, RoutedEventArgs e)
        {
            _viewModel.Disconnect();
            Application.Current.Shutdown();
        }

        private void Click_ShowSettings(object sender, RoutedEventArgs e)
        {
            ToggleMainInterfaceLock(true);
            var slideInAnimation = (Storyboard)FindResource("SlideInAnimation");
            SettingsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void Click_HideSettings(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                SettingsGrid.Visibility = Visibility.Collapsed;
                ToggleMainInterfaceLock(false);
            };
            slideOutAnimation.Begin();
        }

        private static MainMenuViewModel.SearchItem ItemFromSearchButton(Button btn)
        {
            return btn?.DataContext as MainMenuViewModel.SearchItem;
        }

        private static MainMenuViewModel.FriendItem ItemFromFriendButton(Button btn)
        {
            return btn?.DataContext as MainMenuViewModel.FriendItem;
        }

        private void Click_ShowFriends(object sender, RoutedEventArgs e)
        {
            ToggleMainInterfaceLock(true);
            var slideInAnimation = (Storyboard)FindResource("SlideInFriendsAnimation");
            FriendsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();

            _viewModel.LoadInitialFriendData();
        }

        private void Click_HideFriends(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutFriendsAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                FriendsGrid.Visibility = Visibility.Collapsed;
                FriendsAndRequestsView.Visibility = Visibility.Visible;
                SearchView.Visibility = Visibility.Collapsed;
                SearchBox.Text = Lang.socialSearchForAFriend;
                SearchBox.FontStyle = FontStyles.Italic;
                _viewModel.SearchResults.Clear();
                ToggleMainInterfaceLock(false);
            };
            slideOutAnimation.Begin();
        }

        private void Click_ClearSearchBox(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            SearchBox.Focus();
            _viewModel.SearchResults.Clear();

            FriendsAndRequestsView.Visibility = Visibility.Visible;
            SearchView.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Lang.socialSearchForAFriend)
            {
                SearchBox.Text = string.Empty;
                SearchBox.FontStyle = FontStyles.Normal;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = Lang.socialSearchForAFriend;
                SearchBox.FontStyle = FontStyles.Italic;
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (_viewModel.Player?.PlayerID == null)
            {
                return;
            }

            var query = SearchBox.Text?.Trim();

            if (string.IsNullOrEmpty(query) || query == Lang.socialSearchForAFriend)
            {
                FriendsAndRequestsView.Visibility = Visibility.Collapsed;
                SearchView.Visibility = Visibility.Visible;
                return;
            }

            _viewModel.SearchPlayers(query);

            FriendsAndRequestsView.Visibility = Visibility.Collapsed;
            SearchView.Visibility = Visibility.Visible;
        }

        private void Click_ShowGameMode(object sender, RoutedEventArgs e)
        {
            ToggleMainInterfaceLock(true);
            var slideInAnimation = (Storyboard)FindResource("SlideInGameModeAnimation");
            GameModeGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void Click_HideGameMode(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutGameModeAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                GameModeGrid.Visibility = Visibility.Collapsed;
                ToggleMainInterfaceLock(false);
            };
            slideOutAnimation.Begin();
        }

        private void Click_NormalGameMode(object sender, RoutedEventArgs e)
        {
            GoToLobby(GamemodeDM.NORMAL);
            _viewModel.UnsuscribeFromLobbyInvitations();
        }

        private void Click_CustomGameMode(object sender, RoutedEventArgs e)
        {
            GoToLobby(GamemodeDM.CUSTOM);
            _viewModel.UnsuscribeFromLobbyInvitations();
        }

        private void Click_CounterintelligenceMode(object sender, RoutedEventArgs e)
        {
            GoToLobby(GamemodeDM.COUNTERINTELLIGENCE);
            _viewModel.UnsuscribeFromLobbyInvitations();
        }

        private void HandleLobbyInvitationReceived(InvitationReceivedEventArgs e)
        {
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            string message = string.Format(Lang.globalInvitationReceivedMessage, e.Player.Username);

            MessageBoxResult result = MessageBox.Show(message, Lang.globalInvitationReceivedTitle, buttons);

            if (result == MessageBoxResult.Yes)
            {
                AcceptLobbyInvitation(e.LobbyCode);
            }
        }

        private void GoToLobby(GamemodeDM mode)
        {
            LobbyPage lobby = new LobbyPage(_viewModel.Player, mode);
            NavigationService.Navigate(lobby);
            GameModeGrid.Visibility = Visibility.Collapsed;
            ToggleMainInterfaceLock(false);
        }

        private void AcceptLobbyInvitation(string lobbyCode)
        {
            _viewModel.UnsuscribeFromLobbyInvitations();

            LobbyPage lobby = new LobbyPage(_viewModel.Player, GamemodeDM.NORMAL);
            NavigationService.Navigate(lobby);
            GameModeGrid.Visibility = Visibility.Collapsed;
            ToggleMainInterfaceLock(false);
            lobby.AutoJoinLobby(lobbyCode);
        }

        private void Click_ShowScoreboards(object sender, RoutedEventArgs e)
        {
            ToggleMainInterfaceLock(true);
            var slideInAnimation = (Storyboard)FindResource("SlideInScoreboardsAnimation");
            ScoreboardsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();

            _viewModel.OpenScoreboard();
        }

        private void Click_HideScoreboards(object sender, RoutedEventArgs e)
        {
            _viewModel.CloseScoreboard();

            var slideOutAnimation = (Storyboard)FindResource("SlideOutScoreboardsAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                ScoreboardsGrid.Visibility = Visibility.Collapsed;
                ToggleMainInterfaceLock(false);
            };
            slideOutAnimation.Begin();
        }

        private void Click_MyScore(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowMyPersonalScore();
        }

        private void Click_RefreshScoreboard(object sender, RoutedEventArgs e)
        {
            _viewModel.CloseScoreboard();
            _viewModel.OpenScoreboard();
        }

        private void Click_SendRequest(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null)
            {
                return;
            }

            var item = ItemFromSearchButton((Button)sender);
            if (item?.Player?.PlayerID == null)
            {
                return;
            }

            _viewModel.SendFriendRequest(item);
        }

        private void Click_AcceptRequest(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null)
            {
                return;
            }

            var requesterItem = ItemFromFriendButton((Button)sender);
            if (requesterItem?.Player?.PlayerID == null)
            {
                return;
            }

            _viewModel.AcceptFriendRequest(requesterItem);
        }

        private void Click_RejectRequest(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null)
            {
                return;
            }

            var requesterItem = ItemFromFriendButton((Button)sender);
            if (requesterItem?.Player?.PlayerID == null)
            {
                return;
            }

            _viewModel.RejectFriendRequest(requesterItem);
        }

        private void Click_RemoveFriend(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Player?.PlayerID == null)
            {
                return;
            }

            var friendItem = ItemFromFriendButton((Button)sender);
            if (friendItem?.Player?.PlayerID == null)
            {
                return;
            }

            _viewModel.RemoveFriend(friendItem.Player);
        }

        private void Click_OpenFriendProfile(object sender, MouseButtonEventArgs e)
        {
            if (sender is StackPanel panel && panel.DataContext is MainMenuViewModel.FriendItem friendItem)
            {
                Overlay.Visibility = Visibility.Visible;
                _profileControl = new ProfileControl(friendItem.Player, true);

                _profileControl.VerticalAlignment = VerticalAlignment.Center;
                _profileControl.HorizontalAlignment = HorizontalAlignment.Center;
                _profileControl.Visibility = Visibility.Visible;

                _profileControl.ClickCloseProfile += CloseProfile;
                gridMainMenu.Children.Add(_profileControl);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderMaster == null || sliderMusic == null || sliderSFX == null)
            {
                return;
            }

            var slider = sender as Slider;
            double volume0to1 = slider.Value / 100.0;

            if (slider == sliderMaster)
            {
                AudioManager.Instance.SetMasterVolume(volume0to1);
            }
            else if (slider == sliderMusic)
            {
                AudioManager.Instance.SetMusicVolume(volume0to1);
            }
            else if (slider == sliderSFX)
            {
                AudioManager.Instance.SetSfxVolume(volume0to1);
                AudioManager.Instance.PlaySoundEffect("Assets/AudioGame/oof.mp3");
            }
        }

        private void ToggleMainInterfaceLock(bool isLocked)
        {
            if (isLocked)
            {
                Overlay.Visibility = Visibility.Visible;
                stackPanelMainMenu.IsEnabled = false;
            }
            else
            {
                Overlay.Visibility = Visibility.Collapsed;
                stackPanelMainMenu.IsEnabled = true;
            }
        }
    }
}