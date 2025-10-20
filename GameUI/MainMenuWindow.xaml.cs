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
        private PlayerPOCO _player;
        private List<PlayerPOCO> _friends = new List<PlayerPOCO>();
        private List<PlayerPOCO> _requests = new List<PlayerPOCO>();
        private List<PlayerPOCO> _search = new List<PlayerPOCO>();

        public MainMenuWindow()
        {
            InitializeComponent();
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri("Main Theme (NOT FINAL).mp3", UriKind.Relative));
            _mediaPlayer.Play();
            _mediaPlayer.MediaEnded += (s, e) => _mediaPlayer.Position = TimeSpan.Zero;
        }

        private void BtnPlayer_Click(object sender, RoutedEventArgs e)
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
        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInAnimation");
            SettingsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideSettings_Click(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                SettingsGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TO DO
            HideSettings_Click(sender, e);
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowFriends_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInFriendsAnimation");
            FriendsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();

            RefreshFriendsUi();
        }

        private void HideFriends_Click(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutFriendsAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                FriendsGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        private void ClearSearchBox_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            SearchBox.Focus();
            SearchResultsList.ItemsSource = null;
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

        private void ShowGameMode_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInGameModeAnimation");
            GameModeGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideGameMode_Click(object sender, RoutedEventArgs e)
        {
            var slideOutAnimation = (Storyboard)FindResource("SlideOutGameModeAnimation");
            slideOutAnimation.Completed += (s, ev) =>
            {
                GameModeGrid.Visibility = Visibility.Collapsed;
            };
            slideOutAnimation.Begin();
        }

        private void GameMode_Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string gameMode = clickedButton.Content.ToString();
            MessageBox.Show($"Starting game in {gameMode} mode!");
        }

        private void ShowScoreboards_Click(object sender, RoutedEventArgs e)
        {
            var slideInAnimation = (Storyboard)FindResource("SlideInScoreboardsAnimation");
            ScoreboardsGrid.Visibility = Visibility.Visible;
            slideInAnimation.Begin();
        }

        private void HideScoreboards_Click(object sender, RoutedEventArgs e)
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
                PlayerPOCO player = UserOperations.GetPlayer(auxUserID);
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

            _friends = SocialOperations.GetFriends(me);
            _requests = SocialOperations.GetIncomingRequests(me);

            FriendsList.ItemsSource = _friends;
            RequestsList.ItemsSource = _requests;
        }

        private static PlayerPOCO ItemFromButton(Button btn) => btn?.DataContext as PlayerPOCO;

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (_player?.PlayerID == null) return;

            var q = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(q) || q == Lang.socialSearchForAFriend) return;

            var me = _player.PlayerID.Value;
            _search = SocialOperations.SearchPlayers(me, q, 20);
            SearchResultsList.ItemsSource = _search;
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var target = ItemFromButton((Button)sender);
            if (target?.PlayerID == null) return;

            var (ok, msg) = SocialOperations.SendFriendRequest(_player.PlayerID.Value, target.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var requester = ItemFromButton((Button)sender);
            if (requester?.PlayerID == null) return;

            var (ok, msg) = SocialOperations.AcceptFriendRequest(_player.PlayerID.Value, requester.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void RejectRequest_Click(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var requester = ItemFromButton((Button)sender);
            if (requester?.PlayerID == null) return;

            var (ok, msg) = SocialOperations.RejectFriendRequest(_player.PlayerID.Value, requester.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }

        private void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {
            if (_player?.PlayerID == null) return;
            var friend = ItemFromButton((Button)sender);
            if (friend?.PlayerID == null) return;

            var (ok, msg) = SocialOperations.RemoveFriend(_player.PlayerID.Value, friend.PlayerID.Value);
            MessageBox.Show(msg);
            RefreshFriendsUi();
        }
    }
}