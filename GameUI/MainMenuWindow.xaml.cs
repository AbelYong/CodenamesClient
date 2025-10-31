using CodenamesClient.Properties.Langs;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;

namespace CodenamesClient.GameUI
{
    public partial class MainMenuWindow : UserControl
    {
        private MediaPlayer _mediaPlayer;
        private PlayerDM _player;
        private List<PlayerDM> _friends = new List<PlayerDM>();
        private List<PlayerDM> _requests = new List<PlayerDM>();
        private List<PlayerDM> _search = new List<PlayerDM>();

        public MainMenuWindow()
        {
            InitializeComponent();
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri("Main Theme (NOT FINAL).mp3", UriKind.Relative));
            _mediaPlayer.Play();
            _mediaPlayer.MediaEnded += (s, e) => _mediaPlayer.Position = TimeSpan.Zero;
        }

        private void Click_BtnPlayer(object sender, RoutedEventArgs e)
        {
            if (_player != null)
            {
                var w = new ProfileWindow(_player);
                if (w.ShowDialog() == true)
                {
                    SetPlayer(_player.User.UserID);
                }
            }
            else
            {
                MessageBox.Show(Lang.mainMenuGuestCannotAccess);
            }
        }
        private void Click_ShowSettings(object sender, RoutedEventArgs e)
        {
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
            };
            slideOutAnimation.Begin();
        }

        private void Click_SaveButton(object sender, RoutedEventArgs e)
        {
            // TO DO
            Click_HideSettings(sender, e);
        }

        private void Click_BtnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Click_ShowFriends(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInFriendsAnimation");
            FriendsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();

            RefreshFriendsUi();
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
                SearchResultsList.ItemsSource = null;
            };
            slideOutAnimation.Begin();
        }

        private void Click_ClearSearchBox(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            SearchBox.Focus();
            SearchResultsList.ItemsSource = null;

            FriendsAndRequestsView.Visibility = Visibility.Visible;
            SearchView.Visibility = Visibility.Collapsed;
        }

        private void GotFocus_SearchBox(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == Lang.socialSearchForAFriend)
            {
                SearchBox.Text = string.Empty;
                SearchBox.FontStyle = FontStyles.Normal;
            }
        }

        private void LostFocus_SearchBox(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = Lang.socialSearchForAFriend;
                SearchBox.FontStyle = FontStyles.Italic;
            }
        }

        private void Click_ShowGameMode(object sender, RoutedEventArgs e)
        {
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
            };
            slideOutAnimation.Begin();
        }

        private void Click_NormalGameMode(object sender, RoutedEventArgs e)
        {
            GamemodeDM mode = new GamemodeDM(GamemodeDM.GamemodeName.NORMAL);
            GameModeGrid.Visibility = Visibility.Collapsed;
            stackPanelMainMenu.Visibility = Visibility.Collapsed;
            CurrentContent.Content = new LobbyWindow(mode);
        }
        private void Click_CustomGameMode(object sender, RoutedEventArgs e)
        {
            GamemodeDM mode = new GamemodeDM(GamemodeDM.GamemodeName.CUSTOM);
            GameModeGrid.Visibility = Visibility.Collapsed;
            stackPanelMainMenu.Visibility = Visibility.Collapsed;
            CurrentContent.Content = new LobbyWindow(mode);
        }
        private void Click_CounterintelligenceMode(object sender, RoutedEventArgs e)
        {
            GamemodeDM mode = new GamemodeDM(GamemodeDM.GamemodeName.COUNTERINTELLIGENCE);
            GameModeGrid.Visibility = Visibility.Collapsed;
            stackPanelMainMenu.Visibility = Visibility.Collapsed;
            CurrentContent.Content = new LobbyWindow(mode);
        }

        private void Click_ShowScoreboards(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInScoreboardsAnimation");
            ScoreboardsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void Click_HideScoreboards(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutScoreboardsAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                ScoreboardsGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        public void SetPlayer(Guid? userID)
        {
            if (userID != null)
            {
                Guid auxUserID = (Guid) userID;
                PlayerDM player = UserOperation.GetPlayer(auxUserID);
                this._player = player;
                btnPlayer.Content = player.Username;
            }
            else
            {
                btnPlayer.Content = Lang.loginGuest;
            }
        }

        private void RefreshFriendsUi()
        {
            if (_player?.PlayerID == null) return;
            var me = _player.PlayerID.Value;

            _friends = SocialOperation.GetFriends(me);
            _requests = SocialOperation.GetIncomingRequests(me);

            FriendsList.ItemsSource = _friends;
            RequestsList.ItemsSource = _requests;
        }

        private static PlayerDM ItemFromButton(Button btn) => btn?.DataContext as PlayerDM;

        private void KeyDown_SearchBox(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (_player?.PlayerID == null) return;

            var q = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(q) || q == Lang.socialSearchForAFriend) return;

            var me = _player.PlayerID.Value;
            _search = SocialOperation.SearchPlayers(me, q, 20);
            SearchResultsList.ItemsSource = _search;

            FriendsAndRequestsView.Visibility = Visibility.Collapsed;
            SearchView.Visibility = Visibility.Visible;
        }

        private void Click_SendRequest(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var target = ItemFromButton((Button)sender);
            if (target?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.SendFriendRequest(_player.PlayerID.Value, target.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void Click_AcceptRequest(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var requester = ItemFromButton((Button)sender);
            if (requester?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.AcceptFriendRequest(_player.PlayerID.Value, requester.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void Click_RejectRequest(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var requester = ItemFromButton((Button)sender);
            if (requester?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.RejectFriendRequest(_player.PlayerID.Value, requester.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void Click_RemoveFriend(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var friend = ItemFromButton((Button)sender);
            if (friend?.PlayerID == null) return;

            var (ok, msg) = SocialOperation.RemoveFriend(_player.PlayerID.Value, friend.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }
    }
}